using System;
using System.Diagnostics;
using Exceptioneer.WindowsFormsClient;
using NLog;
using NLog.Targets;

namespace NzbDrone.Common
{
    public class ExceptioneerTarget : Target
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Version version = new EnviromentProvider().Version;

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent == null || logEvent.Exception == null || !EnviromentProvider.IsProduction) return;

            Logger.Trace("Sending Exception to Exceptioneer. Process Name: {0}", Process.GetCurrentProcess().ProcessName);

            logEvent.Exception.Data.Add("Version", version.ToString());
            logEvent.Exception.Data.Add("Message", logEvent.Message);


            new Client
                {
                    ApiKey = "43BBF60A-EB2A-4C1C-B09E-422ADF637265",
                    ApplicationName = "NzbDrone",
                    CurrentException = logEvent.Exception
                }.Submit();

        }
    }
}