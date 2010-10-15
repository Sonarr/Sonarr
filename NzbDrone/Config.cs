using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NzbDrone
{
    class Config
    {
        private static string _projectRoot = string.Empty;
        internal static string ProjectRoot
        {
            get
            {
                if (string.IsNullOrEmpty(_projectRoot))
                {
                    var appDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

                    while (appDir.GetDirectories("iisexpress").Length == 0)
                    {
                        if (appDir.Parent == null) throw new ApplicationException("Can't fine IISExpress folder.");
                        appDir = appDir.Parent;
                    }

                    _projectRoot = appDir.FullName;
                }

                return _projectRoot;
            }

        }

        internal static void ConfigureNlog()
        {
            var config = new LoggingConfiguration();

            var debuggerTarget = new DebuggerTarget
            {
                Layout = "${logger}: ${message}"
            };


            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = "${logger}: ${message}"
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

        internal static int Port
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings.Get("port")); }
        }

    }
}
