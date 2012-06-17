using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Exceptron.Driver.Message;

namespace Exceptron.Driver
{
    public class ExceptionClient
    {
        private readonly string _apiKey;
        internal IRestClient RestClient { get; set; }

        /// <summary>
        /// Version of Driver
        /// </summary>
        public string DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }


        /// <summary>
        /// Name of Driver
        /// </summary>
        public string DriverName
        {
            get { return "Official .NET"; }
        }

        /// <summary>
        /// Client Configuration
        /// </summary>
        public ClientConfiguration ClientConfiguration { get; private set; }

        /// <summary>
        /// Environment that the application is running in
        /// </summary>
        /// <example>
        /// Dev, Staging, Production
        /// </example>
        public string Enviroment { get; set; }

        /// <summary>
        /// Version of application executing. Default: Version of <see cref="Assembly.GetEntryAssembly()"/>
        /// </summary>
        public string ApplicationVersion { get; set; }



        /// <param name="apiKey">Your Exceptron API Key</param>
        public ExceptionClient(string apiKey)
            : this(apiKey, new ClientConfiguration())
        {
        }

        /// <param name="apiKey">Your Exceptron API Key</param>
        /// <param name="clientConfiguration">Configuration to use for this client </param>
        private ExceptionClient(string apiKey, ClientConfiguration clientConfiguration)
        {
            ClientConfiguration = clientConfiguration;
            _apiKey = apiKey;

            RestClient = new RestClient();

            SetApplicationVersion();
        }

        /// <summary>
        /// Submit an exception to Exceptron Servers.
        /// </summary>
        /// <param name="exceptionData">Exception data to be reported to the server</param>
        public ExceptionResponse SubmitException(ExceptionData exceptionData)
        {
            try
            {
                if (exceptionData == null)
                    throw new ArgumentNullException("exceptionData");

                if (exceptionData.Exception == null)
                    throw new ArgumentException("ExceptionData.Exception Cannot be null.", "exceptionData");

                var report = new ExceptionReport();

                report.ap = _apiKey;
                report.dn = DriverName;
                report.dv = DriverVersion;
                report.aver = ApplicationVersion;

                report.ext = exceptionData.Exception.GetType().FullName;
                report.stk = ConvertToFrames(exceptionData.Exception);
                report.exm = exceptionData.Exception.Message;

                report.cmp = exceptionData.Component;
                report.uid = exceptionData.UserId;
                report.env = Enviroment;
                report.msg = exceptionData.Message;
                report.cul = Thread.CurrentThread.CurrentCulture.Name;
                report.os = Environment.OSVersion.VersionString;
                report.sv = (int)exceptionData.Severity;

                var response = RestClient.Put<ExceptionResponse>(ClientConfiguration.ServerUrl, report);

                return response;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Unable to submit exception to exceptron. ", e.ToString());

                if (ClientConfiguration.ThrowsExceptions)
                {
                    throw;
                }
                else
                {
                    return new ExceptionResponse { Exception = e };
                }
            }
        }


        private void SetApplicationVersion()
        {
            try
            {
                var entryAssembly = GetWebEntryAssembly();

                if (entryAssembly == null)
                {
                    entryAssembly = Assembly.GetEntryAssembly();
                }

                if (entryAssembly == null)
                {
                    entryAssembly = Assembly.GetCallingAssembly();
                }

                if (entryAssembly != null)
                {
                    ApplicationVersion = entryAssembly.GetName().Version.ToString();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Can't figure out application version.", e.ToString());
            }
        }

        static private Assembly GetWebEntryAssembly()
        {
            if (System.Web.HttpContext.Current == null ||
                System.Web.HttpContext.Current.ApplicationInstance == null)
            {
                return null;
            }

            var type = System.Web.HttpContext.Current.ApplicationInstance.GetType();
            while (type != null && type.Namespace == "ASP")
            {
                type = type.BaseType;
            }

            return type == null ? null : type.Assembly;
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

    }
}
