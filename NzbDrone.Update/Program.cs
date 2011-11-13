using System;
using System.Linq;
using NLog;
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

                LogConfiguration.RegisterConsoleLogger(LogLevel.Trace);
                LogConfiguration.RegisterUdpLogger();
                LogConfiguration.RegisterExceptioneer();
                LogConfiguration.Reload();

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

        public void Start(string[] args)
        {
            VerfityArguments(args);
            int processId = ParseProcessId(args);

            string appPath = _processProvider.GetProcessById(processId).StartPath;

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
