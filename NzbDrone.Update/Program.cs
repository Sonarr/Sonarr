using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Composition;
using NzbDrone.Update.UpdateEngine;

namespace NzbDrone.Update
{
    public class Program
    {
        private readonly IInstallUpdateService _installUpdateService;
        private readonly IProcessProvider _processProvider;
        private static IContainer _container;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Program(IInstallUpdateService installUpdateService, IProcessProvider processProvider)
        {
            _installUpdateService = installUpdateService;
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

        }

        public void Start(string[] args)
        {
            int processId = ParseProcessId(args);

            var exeFileInfo = new FileInfo(_processProvider.GetProcessById(processId).StartPath);
            string targetFolder = exeFileInfo.Directory.FullName;

            logger.Info("Starting update process. Target Path:{0}", targetFolder);
            _installUpdateService.Start(targetFolder);
        }

        private int ParseProcessId(string[] args)
        {
            int id;
            if (args == null || !Int32.TryParse(args[0], out id) || id <= 0)
            {
                throw new ArgumentOutOfRangeException("args", "Invalid process ID");
            }

            logger.Debug("NzbDrone processId:{0}", id);
            return id;
        }
    }
}
