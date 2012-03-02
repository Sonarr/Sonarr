using System;
using System.IO;
using System.Linq;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update
{
    public class Program
    {
        private readonly UpdateProvider _updateProvider;
        private readonly ProcessProvider _processProvider;
        private static StandardKernel _kernel;

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
                _kernel = new StandardKernel();
                InitLoggers();

   

                logger.Info("Updating NzbDrone to version {0}", _kernel.Get<EnviromentProvider>().Version);
                _kernel.Get<Program>().Start(args);
            }
            catch (Exception e)
            {
                logger.FatalException("An error has occurred while applying update package.", e);
            }

            TransferUpdateLogs();

        }

        private static void TransferUpdateLogs()
        {
            try
            {
                var enviromentProvider = _kernel.Get<EnviromentProvider>();
                var diskProvider = _kernel.Get<DiskProvider>();
                logger.Info("Copying log files to application directory.");
                diskProvider.CopyDirectory(enviromentProvider.GetSandboxLogFolder(), enviromentProvider.GetUpdateLogFolder());
            }
            catch (Exception e)
            {
                logger.FatalException("Can't copy upgrade log files to target folder", e);
            }
        }

        private static void InitLoggers()
        {
            ReportingService.RestProvider = _kernel.Get<RestProvider>();

            LogConfiguration.RegisterRemote();

            LogConfiguration.RegisterConsoleLogger(LogLevel.Trace);
            LogConfiguration.RegisterUdpLogger();

            var logPath = Path.Combine(new EnviromentProvider().GetSandboxLogFolder(), DateTime.Now.ToString("yyyy.MM.dd-H-mm") + ".txt");
            LogConfiguration.RegisterFileLogger(logPath, LogLevel.Info);
            
            LogConfiguration.Reload();
        }

        public void Start(string[] args)
        {
            VerfityArguments(args);
            int processId = ParseProcessId(args);

            var exeFileInfo = new FileInfo(_processProvider.GetProcessById(processId).StartPath);
            string targetFolder = exeFileInfo.Directory.FullName;

            logger.Info("Starting update process. Target Path:{0}", targetFolder);
            _updateProvider.Start(targetFolder);
        }

        private int ParseProcessId(string[] args)
        {
            int id;
            if (!Int32.TryParse(args[0], out id) || id <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid process id: " + args[0]);
            }

            logger.Debug("NzbDrone processId:{0}", id);
            return id;
        }

        private void VerfityArguments(string[] args)
        {
            if (args == null || args.Length != 2)
                throw new ArgumentException("Wrong number of parameters were passed in.");

            logger.Debug("Arguments verified. [{0}] , [{1}]", args[0], args[1]);
        }
    }
}
