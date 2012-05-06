using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Exceptron.Driver.Message;
using Exceptron.Driver.fastJSON;

namespace Exceptron.Driver
{
    public class ExceptionClient
    {
        public readonly string DriverVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public readonly string DriverName = "Official .NET";

        private readonly string _appId;

        public bool ThrowsExceptions { get; set; }
        public string Enviroment { get; set; }
        public string ApplicationVersion { get; set; }

        internal RestClient RestClient { get; set; }
        internal string ServerUrl { get; set; }


        public ExceptionClient(string appId)
        {
            _appId = appId;

            ServerUrl = "http://localhost:57674/v1a/";



            RestClient = new RestClient();
        }


        internal static List<Frame> ConvertToFrames(Exception exception)
        {
            if (exception == null) return null;

            var stackTrace = new StackTrace(exception, true);

            var frames = stackTrace.GetFrames();

            if (frames == null) return null;

            var result = new List<Frame>();

            for (int index = 0; index < frames.Length; index++)
            {
                var frame = frames[index];
                var method = frame.GetMethod();
                var declaringType = method.DeclaringType;

                var currentFrame = new Frame
                                       {
                                           i = index,
                                           fn = frame.GetFileName(),
                                           ln = frame.GetFileLineNumber(),
                                           m = method.ToString(),
                                       };


                currentFrame.m = currentFrame.m.Substring(currentFrame.m.IndexOf(' ')).Trim();


                if (declaringType != null)
                {
                    currentFrame.c = declaringType.FullName;
                }

                result.Add(currentFrame);
            }


            return result;
        }

        public string SubmitException(ExceptionData exceptionData)
        {
            try
            {
                VerifyErrorData(exceptionData);

                var report = new ExceptionReport();

                report.ap = _appId;
                report.dn = DriverName;
                report.dv = DriverVersion;
                report.aver = ApplicationVersion;

                report.ext = exceptionData.Exception.GetType().FullName;
                report.exm = exceptionData.Exception.Message;
                report.stk = ConvertToFrames(exceptionData.Exception);

                report.cmp = exceptionData.Component;
                report.uid = exceptionData.UserId;
                report.env = Enviroment;
                report.msg = exceptionData.Message;
                report.cul = Thread.CurrentThread.CurrentCulture.Name;
                report.os = Environment.OSVersion.VersionString;

                var messageString = JSON.Instance.ToJSON(report);
                var response = RestClient.Put(ServerUrl, messageString);
                return response;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Unable to submit exception to exceptron. ", e.ToString());

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

    }
}
