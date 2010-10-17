using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Exceptioneer.WindowsFormsClient;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NzbDrone.Core
{
    public static class Instrumentation
    {
        public static void Setup()
        {
            if (Debugger.IsAttached)
            {
                LogManager.ThrowExceptions = true;
            }

            LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(CentralDispatch.AppPath, "log.config"), false);
            LogManager.ConfigurationReloaded += ((s, e) => BindExceptioneer());
            BindExceptioneer();
        }

        private static void BindExceptioneer()
        {
            var exTarget = new ExceptioneerTarget();
            LogManager.Configuration.AddTarget("Exceptioneer", exTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, exTarget));
            LogManager.Configuration.Reload();
        }



        public class NlogWriter : TextWriter
        {
            private static readonly Logger Logger = LogManager.GetLogger("DB");


            public override void Write(char[] buffer, int index, int count)
            {
                Write(new string(buffer, index, count));
            }

            public override void Write(string value)
            {
                DbAction(value);
            }

            private static void DbAction(string value)
            {
                Logger.Trace(value);
            }

            public override Encoding Encoding
            {
                get { return Encoding.Default; }
            }
        }


        public class ExceptioneerTarget : Target
        {
            protected override void Write(LogEventInfo logEvent)
            {
                if (logEvent.Exception == null)
                    throw new InvalidOperationException(@"Missing Exception Object.. Please Use Logger.FatalException() or Logger.ErrorException() rather
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
}



