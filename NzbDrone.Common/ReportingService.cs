using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Contract;

namespace NzbDrone.Common
{
    public static class ReportingService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string SERVICE_URL = "http://services.nzbdrone.com/reporting";
        private const string PARSE_URL = SERVICE_URL + "/ParseError";
        private const string EXCEPTION_URL = SERVICE_URL + "/ReportException";

        public static RestProvider RestProvider { get; set; }
        private static readonly HashSet<string> parserErrorCache = new HashSet<string>();


        public static void ClearCache()
        {
            lock (parserErrorCache)
            {
                parserErrorCache.Clear();
            }
        }

        public static void ReportParseError(string title)
        {
            try
            {
                VerifyRestProvider();

                lock (parserErrorCache)
                {
                    if (parserErrorCache.Contains(title.ToLower())) return;
                    
                    parserErrorCache.Add(title.ToLower());
                }

                var report = new ParseErrorReport { Title = title };
                RestProvider.PostData(PARSE_URL, report);
            }
            catch (Exception e)
            {
                if (!EnvironmentProvider.IsProduction)
                {
                    throw;
                }

                e.Data.Add("title", title);
                logger.InfoException("Unable to report parse error", e);
            }
        }

        public static void ReportException(LogEventInfo logEvent)
        {
            try
            {
                VerifyRestProvider();
                    
                var report = new ExceptionReport();
                report.LogMessage = logEvent.FormattedMessage;
                report.Stack = logEvent.Exception.StackTrace;
                report.ExceptionMessage = logEvent.Exception.Message;
                report.Logger = logEvent.LoggerName;
                report.Type = logEvent.Exception.GetType().Name;

                RestProvider.PostData(EXCEPTION_URL, report);
            }
            catch (Exception e)
            {
                if (!EnvironmentProvider.IsProduction)
                {
                    throw;
                }

                //this shouldn't log an exception since it will cause a recursive loop.
                logger.Info("Unable to report exception. " + e);
            }
        }

        private static void VerifyRestProvider()
        {
            if(RestProvider == null)
            {
                if(EnvironmentProvider.IsProduction)
                {
                    logger.Warn("Rest provider wasn't provided. creating new one!");
                    RestProvider = new RestProvider(new EnvironmentProvider());
                }
                else
                {
                    throw new InvalidOperationException("REST Provider wasn't configured correctly.");
                }
            }
        }
    }
}
