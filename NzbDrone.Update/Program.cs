using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Composition;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update
{
    public class Program
    {
        private readonly UpdateProvider _updateProvider;
        private readonly IProcessProvider _processProvider;
        private static IContainer _container;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Program(UpdateProvider updateProvider, IProcessProvider processProvider)
        {
            _updateProvider = updateProvider;
            _processProvider = processProvider;
        }

        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting NzbDrone Update Client");

                _container = UpdateContainerBuilder.Build();

                logger.Info("Updating NzbDrone to version {0}", _container.Resolve<IEnvironmentProvider>().Version);
                _container.Resolve<Program>().Start(args);
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
                var environmentProvider = _container.Resolve<IEnvironmentProvider>();
                var diskProvider = _container.Resolve<IDiskProvider>();
                logger.Info("Copying log files to application directory.");
                diskProvider.CopyDirectory(environmentProvider.GetSandboxLogFolder(), environmentProvider.GetUpdateLogFolder());
            }
            catch (Exception e)
            {
                logger.FatalException("Can't copy upgrade log files to target folder", e);
            }
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
