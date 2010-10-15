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
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            const string callSight = "${callsite:className=false:fileName=false:includeSourcePath=false:methodName=true}";
            string layout = string.Concat("[${logger}](", callSight, "): ${message}");
            // Step 2. Create targets and add them to the configuration 
            var debuggerTarget = new DebuggerTarget
            {
                Layout = layout
            };

            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = layout
            };

            var fileTarget = new FileTarget
            {
                FileName = "${basedir}/test.log",
                Layout = layout
            };

            config.AddTarget("debugger", debuggerTarget);
            config.AddTarget("console", consoleTarget);
            //config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            // Step 4. Define rules
            //LoggingRule fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            var debugRule = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
            var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);

            //config.LoggingRules.Add(fileRule);
            config.LoggingRules.Add(debugRule);
            config.LoggingRules.Add(consoleRule);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }

        public static void LogEpicException(Exception e)
        {
            try
            {
                LogManager.GetLogger("EPICFAIL").FatalException("Unhandled Exception", e);
            }
            catch (Exception totalFailException)
            {
                Console.WriteLine("TOTAL FAIL:{0}", totalFailException);
                Console.WriteLine(e.ToString());
            }

            PublishExceptoion(e);
        }


        private static bool PublishExceptoion(Exception e)
        {
            //Don't publish exceptions when debugging the app.
            if (Debugger.IsAttached)
                return false;

            return new Client
                {
                    ApiKey = "43BBF60A-EB2A-4C1C-B09E-422ADF637265",
                    ApplicationName = "NZBDrone",
                    CurrentException = e
                }.Submit();
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
    }


}
