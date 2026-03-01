using System;
using System.Diagnostics;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Sentry;

namespace NzbDrone.Common.Instrumentation
{
    public static class NzbDroneLogger
    {
        private const string FileLogLayout = @"${date:format=yyyy-MM-dd HH\:mm\:ss.f}|${level}|${logger}|${message}${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}}";
        private const string ConsoleFormat = "[${level}] ${logger}: ${message} ${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}}";

        private static readonly CleansingConsoleLogLayout CleansingConsoleLayout = new(ConsoleFormat);
        private static readonly CleansingClefLogLayout ClefLogLayout = new();

        private static bool _isConfigured;

        static NzbDroneLogger()
        {
            LogManager.Configuration = new LoggingConfiguration();
        }

        public static void Register(IStartupContext startupContext, bool updateApp, bool inConsole)
        {
            if (_isConfigured)
            {
                throw new InvalidOperationException("Loggers have already been registered.");
            }

            _isConfigured = true;

            GlobalExceptionHandlers.Register();

            var appFolderInfo = new AppFolderInfo(startupContext);

            RegisterGlobalFilters();

            if (Debugger.IsAttached)
            {
                RegisterDebugger();
            }

            RegisterSentry(updateApp, appFolderInfo);

            if (updateApp)
            {
                RegisterUpdateFile(appFolderInfo);
            }
            else
            {
                if (inConsole && (OsInfo.IsNotWindows || RuntimeInfo.IsUserInteractive))
                {
                    RegisterConsole();
                }

                RegisterAppFile(appFolderInfo);
            }

            RegisterAuthLogger();

            LogManager.ReconfigExistingLoggers();
        }

        private static void RegisterSentry(bool updateClient, IAppFolderInfo appFolderInfo)
        {
            string dsn;

            if (updateClient)
            {
                dsn = RuntimeInfo.IsProduction
                    ? "https://80777986b95f44a1a90d1eb2f3af1e36@sentry.sonarr.tv/11"
                    : "https://6168f0946aba4e60ac23e469ac08eac5@sentry.sonarr.tv/9";
            }
            else
            {
                dsn = RuntimeInfo.IsProduction
                    ? "https://e2adcbe52caf46aeaebb6b1dcdfe10a1@sentry.sonarr.tv/8"
                    : "https://4ee3580e01d8407c96a7430fbc953512@sentry.sonarr.tv/10";
            }

            Target target;
            try
            {
                target = new SentryTarget(dsn, appFolderInfo)
                {
                    Name = "sentryTarget",
                    Layout = "${message}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load dependency, may need an OS update: " + ex.ToString());
                LogManager.GetLogger(nameof(NzbDroneLogger)).Debug(ex, "Failed to load dependency, may need an OS update");

                // We still need the logging rules, so use a null target.
                target = new NullTarget();
            }

            var loggingRule = new LoggingRule("*", updateClient ? LogLevel.Trace : LogLevel.Warn, target);
            LogManager.Configuration.AddTarget("sentryTarget", target);
            LogManager.Configuration.LoggingRules.Add(loggingRule);

            // Events logged to Sentry go only to Sentry.
            var loggingRuleSentry = new LoggingRule("Sentry", LogLevel.Debug, target) { Final = true };
            LogManager.Configuration.LoggingRules.Insert(0, loggingRuleSentry);
        }

        private static void RegisterDebugger()
        {
            var target = new DebuggerTarget();
            target.Name = "debuggerLogger";
            target.Layout = "[${level}] [${threadid}] ${logger}: ${message} ${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}}";

            var loggingRule = new LoggingRule("*", LogLevel.Trace, target);
            LogManager.Configuration.AddTarget("debugger", target);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private static void RegisterConsole()
        {
            var level = LogLevel.Trace;

            var coloredConsoleTarget = new ColoredConsoleTarget();

            coloredConsoleTarget.Name = "consoleLogger";

            var logFormat = Enum.TryParse<ConsoleLogFormat>(Environment.GetEnvironmentVariable("SONARR__LOG__CONSOLEFORMAT"), out var formatEnumValue)
                ? formatEnumValue
                : ConsoleLogFormat.Standard;

            ConfigureConsoleLayout(coloredConsoleTarget, logFormat);

            var loggingRule = new LoggingRule("*", level, coloredConsoleTarget);

            LogManager.Configuration.AddTarget("console", coloredConsoleTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private static void RegisterAppFile(IAppFolderInfo appFolderInfo)
        {
            RegisterAppFile(appFolderInfo, "appFileInfo", "sonarr.txt", 5, LogLevel.Info);
            RegisterAppFile(appFolderInfo, "appFileDebug", "sonarr.debug.txt", 50, LogLevel.Off);
            RegisterAppFile(appFolderInfo, "appFileTrace", "sonarr.trace.txt", 50, LogLevel.Off);
        }

        private static void RegisterAppFile(IAppFolderInfo appFolderInfo, string name, string fileName, int maxArchiveFiles, LogLevel minLogLevel)
        {
            var fileTarget = new CleansingFileTarget();

            fileTarget.Name = name;
            fileTarget.FileName = Path.Combine(appFolderInfo.GetLogFolder(), fileName);
            fileTarget.AutoFlush = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ConcurrentWriteAttemptDelay = 50;
            fileTarget.ConcurrentWriteAttempts = 10;
            fileTarget.ArchiveAboveSize = 1.Megabytes();
            fileTarget.MaxArchiveFiles = maxArchiveFiles;
            fileTarget.EnableFileDelete = true;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
            fileTarget.Layout = FileLogLayout;

            var loggingRule = new LoggingRule("*", minLogLevel, fileTarget);

            LogManager.Configuration.AddTarget(name, fileTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private static void RegisterUpdateFile(IAppFolderInfo appFolderInfo)
        {
            var fileTarget = new FileTarget();

            fileTarget.Name = "updateFileLogger";
            fileTarget.FileName = Path.Combine(appFolderInfo.GetUpdateLogFolder(), DateTime.Now.ToString("yyyy.MM.dd-HH.mm") + ".txt");
            fileTarget.AutoFlush = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ConcurrentWriteAttemptDelay = 50;
            fileTarget.ConcurrentWriteAttempts = 100;
            fileTarget.Layout = FileLogLayout;

            var loggingRule = new LoggingRule("*", LogLevel.Trace, fileTarget);

            LogManager.Configuration.AddTarget("updateFile", fileTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private static void RegisterAuthLogger()
        {
            var consoleTarget = LogManager.Configuration.FindTargetByName("console");
            var fileTarget = LogManager.Configuration.FindTargetByName("appFileInfo");

            var target = consoleTarget ?? fileTarget ?? new NullTarget();

            // Send Auth to Console and info app file, but not the log database
            var rule = new LoggingRule("Auth", LogLevel.Info, target) { Final = true };
            if (consoleTarget != null && fileTarget != null)
            {
                rule.Targets.Add(fileTarget);
            }

            LogManager.Configuration.LoggingRules.Insert(0, rule);
        }

        private static void RegisterGlobalFilters()
        {
            LogManager.Setup().LoadConfiguration(c =>
            {
                c.ForLogger("System.*").WriteToNil(LogLevel.Warn);
                c.ForLogger("Microsoft.*").WriteToNil(LogLevel.Warn);
                c.ForLogger("Microsoft.Hosting.Lifetime*").WriteToNil(LogLevel.Info);
                c.ForLogger("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware").WriteToNil(LogLevel.Fatal);
                c.ForLogger("Sonarr.Http.Authentication.ApiKeyAuthenticationHandler").WriteToNil(LogLevel.Info);
            });
        }

        public static Logger GetLogger(Type obj)
        {
            return LogManager.GetLogger(obj.Name.Replace("NzbDrone.", ""));
        }

        public static Logger GetLogger(object obj)
        {
            return GetLogger(obj.GetType());
        }

        public static void ConfigureConsoleLayout(ColoredConsoleTarget target, ConsoleLogFormat format)
        {
            target.Layout = format switch
            {
                ConsoleLogFormat.Clef => NzbDroneLogger.ClefLogLayout,
                _ => NzbDroneLogger.CleansingConsoleLayout
            };
        }

        public static void ResetAllTargets(IStartupContext startupContext, bool updateApp, bool inConsole)
        {
            LogManager.Configuration = new LoggingConfiguration();
            _isConfigured = false;
            Register(startupContext, updateApp, inConsole);
        }
    }

    public enum ConsoleLogFormat
    {
        Standard,
        Clef
    }
}
