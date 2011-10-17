using System.Diagnostics;
using Exceptioneer.WindowsFormsClient;
using NLog;
using NLog.Targets;

namespace NzbDrone.Core.Instrumentation
{
    public class ExceptioneerTarget : Target
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent == null || logEvent.Exception == null) return;
            if (Debugger.IsAttached || Process.GetCurrentProcess().ProcessName.Contains("JetBrains")) return;
            
            Logger.Trace("Sending Exception to Exceptioneer. {0}", Process.GetCurrentProcess().ProcessName);

            new Client
                {
                    ApiKey = "43BBF60A-EB2A-4C1C-B09E-422ADF637265",
                    ApplicationName = "NZBDrone",
                    CurrentException = logEvent.Exception
                }.Submit();

        }
    }
}