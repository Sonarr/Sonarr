using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Processes;
using NzbDrone.Common.Security;
using NzbDrone.Update.UpdateEngine;

namespace NzbDrone.Update
{
    public class UpdateApp
    {
        private readonly IInstallUpdateService _installUpdateService;
        private readonly IProcessProvider _processProvider;
        private static IContainer _container;

        private static readonly Logger logger =  NzbDroneLogger.GetLogger();

        public UpdateApp(IInstallUpdateService installUpdateService, IProcessProvider processProvider)
        {
            _installUpdateService = installUpdateService;
            _processProvider = processProvider;
        }

        public static void Main(string[] args)
        {
            try
            {
                var startupArgument = new StartupContext(args);
                LogTargets.Register(startupArgument, true, true);

                Console.WriteLine("Starting NzbDrone Update Client");

                IgnoreCertErrorPolicy.Register();

                GlobalExceptionHandlers.Register();

                _container = UpdateContainerBuilder.Build(startupArgument);

                logger.Info("Updating NzbDrone to version {0}", BuildInfo.Version);
                _container.Resolve<UpdateApp>().Start(args);
            }
            catch (Exception e)
            {
                logger.FatalException("An error has occurred while applying update package.", e);
            }
        }

        public void Start(string[] args)
        {
            var startupContext = ParseArgs(args);
            string targetFolder;

            if (startupContext.ExecutingApplication.IsNullOrWhiteSpace())
            {
                var exeFileInfo = new FileInfo(_processProvider.GetProcessById(startupContext.ProcessId).StartPath);
                targetFolder = exeFileInfo.Directory.FullName;
            }

            else
            {
                var exeFileInfo = new FileInfo(startupContext.ExecutingApplication);
                targetFolder = exeFileInfo.Directory.FullName;
            }
            
            logger.Info("Starting update process. Target Path:{0}", targetFolder);
            _installUpdateService.Start(targetFolder, startupContext.ProcessId);
        }

        private UpdateStartupContext ParseArgs(string[] args)
        {
            if (args == null || !args.Any())
            {
                throw new ArgumentOutOfRangeException("args", "args must be specified");
            }

            var startupContext = new UpdateStartupContext
                                 {
                                     ProcessId = ParseProcessId(args[0])
                                 };

            if (args.Count() == 1)
            {
                return startupContext;
            }

            if (args.Count() >= 3)
            {
                startupContext.UpdateLocation = args[1];
                startupContext.ExecutingApplication = args[2];
            }

            return startupContext;
        }

        private int ParseProcessId(string arg)
        {
            int id;
            if (!Int32.TryParse(arg, out id) || id <= 0)
            {
                throw new ArgumentOutOfRangeException("arg", "Invalid process ID");
            }

            logger.Debug("NzbDrone process ID: {0}", id);
            return id;
        }
    }
}
