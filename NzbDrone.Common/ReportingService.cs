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
                if (RestProvider == null && EnviromentProvider.IsProduction)
                    return;

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
                if (!EnviromentProvider.IsProduction)
                {
                    throw;
                }

                e.Data.Add("title", title);
                logger.ErrorException("Unable to report parse error", e);
            }
        }

        public static void ReportException(LogEventInfo logEvent)
        {
            try
            {
                if (RestProvider == null && EnviromentProvider.IsProduction)
                    return;

                var report = new ExceptionReport();
                report.LogMessage = logEvent.FormattedMessage;
                report.String = logEvent.Exception.ToString();
                report.Logger = logEvent.LoggerName;
                report.Type = logEvent.Exception.GetType().Name;

                RestProvider.PostData(EXCEPTION_URL, report);
            }
            catch (Exception e)
            {
                if (!EnviromentProvider.IsProduction)
                {
                    throw;
                }

                //this shouldn't log an exception since it might cause a recursive loop.
                logger.Error("Unable to report exception. " + e);
            }
        }
    }
}
