using System;
using System.Diagnostics;
using Exceptioneer.WindowsFormsClient;
using NLog;
using NLog.Targets;

namespace NzbDrone.Core.Instrumentation
{
    public class ExceptioneerTarget : Target
    {
        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent.Exception == null)
                throw new InvalidOperationException(
                    @"Missing Exception Object.. Please Use Logger.FatalException() or Logger.ErrorException() rather
                than Logger.Fatal() and Logger.Error()");

            if (!Debugger.IsAttached)
            {
                new Client
                    {
                        ApiKey = "43BBF60A-EB2A-4C1C-B09E-422ADF637265",
                        ApplicationName = "NZBDrone",
                        CurrentException = logEvent.Exception
                    }.Submit();
            }
        }
    }
}