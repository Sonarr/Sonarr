using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Exceptron.Driver.fastJSON;

namespace Exceptron.Driver
{
    public class ExceptionClient
    {
        private const string DriverName = "Official .NET";

        private static readonly string DriverVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private readonly string _serverUrl;
        private readonly string _appId;
        private readonly string _appVersion;

        public bool ThrowsExceptions { get; set; }
        public string Enviroment { get; set; }

        public ExceptionClient(string appId, string appVersion, Uri serverUrl)
        {
            _appId = appId;
            _appVersion = appVersion;
            _serverUrl = serverUrl.ToString();
        }


        private static List<Frame> GetExceptionFrames(Exception exception)
        {
            if (exception == null) return null;

            var stackTrace = new StackTrace(exception, true);

            var frames = stackTrace.GetFrames();

            if (frames == null) return null;

            var result = new List<Frame>();

            foreach (var frame in frames)
            {
                result.Add(new Frame { FileName = frame.GetFileName(), LineNumber = frame.GetFileLineNumber(), Method = frame.GetMethod().Name });
            }


            return result;
        }

        public string SubmitException(ExceptionData exceptionData)
        {
            try
            {
                VerifyErrorData(exceptionData);

                var report = new ExceptionReport();

                report.AppId = _appId;
                report.DriverName = DriverName;
                report.DriverVersion = DriverVersion;
                report.AppVersion = _appVersion;

                report.ExceptionType = exceptionData.Exception.GetType().FullName;
                report.ExceptionMessage = exceptionData.Exception.Message;
                report.StackTrace = GetExceptionFrames(exceptionData.Exception);

                report.Location = exceptionData.Location;
                report.Uid = exceptionData.UserId;
                report.Enviroment = Enviroment;
                report.Message = exceptionData.Message;

                var response = PutObject(report);
                return response;
            }
            catch (Exception)
            {
                if (ThrowsExceptions) throw;
                return null;
            }
        }

        private void VerifyErrorData(ExceptionData exceptionData)
        {
            if (exceptionData == null)
                throw new ArgumentNullException("exceptionData");

            if (exceptionData.Exception == null)
                throw new ArgumentException("ExceptionData.Exception Cannot be null.", "exceptionData");
        }

        private string PutObject(ExceptionReport exceptionReport)
        {
            string exceptionData = JSON.Instance.ToJSON(exceptionReport);
            byte[] bytes = Encoding.UTF8.GetBytes(exceptionData);
            var request = (HttpWebRequest)WebRequest.Create(_serverUrl);
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.ContentLength = bytes.Length;
            request.Accept = "application/json";

            var dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();

            var responseStream = new StreamReader(request.GetResponse().GetResponseStream(), Encoding.GetEncoding(1252));

            string response = responseStream.ReadToEnd();

            return response;
        }
    }
}
