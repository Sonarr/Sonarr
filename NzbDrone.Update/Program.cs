using System;
using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Common;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update
{
    public class Program
    {
        private readonly UpdateProvider _updateProvider;
        private readonly ProcessProvider _processProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Program(UpdateProvider updateProvider, ProcessProvider processProvider)
        {
            _updateProvider = updateProvider;
            _processProvider = processProvider;
        }

        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting NzbDrone Update Client");

                InitLoggers();

                logger.Info("Initializing update application");

                var enviromentProvider = new EnviromentProvider();
                var processProvider = new ProcessProvider();
                var serviceProvider = new ServiceProvider();
                var diskProvider = new DiskProvider();

                var updateProvider = new UpdateProvider(diskProvider, serviceProvider, processProvider, enviromentProvider);

                new Program(updateProvider, processProvider).Start(args);
            }
            catch (Exception e)
            {
                logger.FatalException("An error has occurred while applying update package.", e);
            }
        }

        private static void InitLoggers()
        {
            LogConfiguration.RegisterConsoleLogger(LogLevel.Trace);
            LogConfiguration.RegisterUdpLogger();
            LogConfiguration.RegisterExceptioneer();

            var fileTarget = new FileTarget();
            fileTarget.AutoFlush = true;
            fileTarget.ConcurrentWrites = false;
            fileTarget.DeleteOldFileOnStartup = true;
            fileTarget.FileName = "upgrade.log";
            fileTarget.KeepFileOpen =false;
            
            fileTarget.Layout = "${logger}: ${message} ${exception}";
            LogManager.Configuration.AddTarget(fileTarget.GetType().Name, fileTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, fileTarget));
            
            LogConfiguration.Reload();
        }

        public void Start(string[] args)
        {
            VerfityArguments(args);
            int processId = ParseProcessId(args);

            FileInfo exeFileInfo = new FileInfo(_processProvider.GetProcessById(processId).StartPath);
            string appPath = exeFileInfo.Directory.FullName;

            logger.Info("Starting update process");
            _updateProvider.Start(appPath);
        }

        private int ParseProcessId(string[] args)
        {
            int id = 0;
            if (!Int32.TryParse(args[0], out id) || id <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid process id: " + args[0]);
            }

            return id;
        }

        private void VerfityArguments(string[] args)
        {
            if (args == null || args.Length != 2)
                throw new ArgumentException("Wrong number of parameters were passed in.");
        }
    }
}
