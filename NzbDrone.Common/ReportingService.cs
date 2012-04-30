using System;
using System.Collections.Generic;
using System.Linq;
using Exceptron.Driver;
using NLog;
using NzbDrone.Common.Contract;

namespace NzbDrone.Common
{
    public static class ReportingService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string SERVICE_URL = "http://services.nzbdrone.com/reporting";
        private const string PARSE_URL = SERVICE_URL + "/ParseError";

        public static RestProvider RestProvider { get; set; }
        public static ExceptionClient ExceptronDriver { get; set; }


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
                VerifyDependencies();

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

        public static string ReportException(LogEventInfo logEvent)
        {
            try
            {
                VerifyDependencies();

                var exceptionData = new ExceptionData();

                exceptionData.Exception = logEvent.Exception;
                exceptionData.Location = logEvent.LoggerName;
                exceptionData.Message = logEvent.FormattedMessage;
                exceptionData.UserId = EnvironmentProvider.UGuid.ToString().Replace("-", string.Empty);

                return ExceptronDriver.SubmitException(exceptionData);
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

            return null;
        }


        public static void SetupExceptronDriver()
        {
            ExceptronDriver = new ExceptionClient(
                    "CB230C312E5C4FF38B4FB9644B05E60E",
                    new EnvironmentProvider().Version.ToString(),
                    new Uri("http://api.Exceptron.com/v1aa/"));

            ExceptronDriver.ThrowsExceptions = !EnvironmentProvider.IsProduction;
            ExceptronDriver.Enviroment = EnvironmentProvider.IsProduction ? "Prod" : "Dev";
        }

        private static void VerifyDependencies()
        {
            if (RestProvider == null)
            {
                if (EnvironmentProvider.IsProduction)
                {
                    logger.Warn("Rest provider wasn't provided. creating new one!");
                    RestProvider = new RestProvider(new EnvironmentProvider());
                }
                else
                {
                    throw new InvalidOperationException("REST Provider wasn't configured correctly.");
                }
            }

            if (ExceptronDriver == null)
            {
                if (EnvironmentProvider.IsProduction)
                {
                    logger.Warn("Exceptron Driver wasn't provided. creating new one!");
                    SetupExceptronDriver();
                }
                else
                {
                    throw new InvalidOperationException("Exceptron Driver wasn't configured correctly.");
                }
            }
        }
    }
}
