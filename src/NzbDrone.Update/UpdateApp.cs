using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Processes;
using NzbDrone.Update.UpdateEngine;

namespace NzbDrone.Update
{
    public class UpdateApp
    {
        private readonly IInstallUpdateService _installUpdateService;
        private readonly IProcessProvider _processProvider;

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(UpdateApp));

        private static IContainer _container;

        public UpdateApp(IInstallUpdateService installUpdateService, IProcessProvider processProvider)
        {
            _installUpdateService = installUpdateService;
            _processProvider = processProvider;
        }

        public static void Main(string[] args)
        {
            try
            {
                var startupContext = new StartupContext(args);
                NzbDroneLogger.Register(startupContext, true, true);

                Logger.Info("Starting Sonarr Update Client");

                _container = UpdateContainerBuilder.Build(startupContext);
                _container.Resolve<InitializeLogger>().Initialize();
                _container.Resolve<UpdateApp>().Start(args);

                Logger.Info("Update completed successfully");
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "An error has occurred while applying update package.");
            }
        }

        public void Start(string[] args)
        {
            var startupContext = ParseArgs(args);
            var targetFolder = GetInstallationDirectory(startupContext);

            _installUpdateService.Start(targetFolder, startupContext.ProcessId);
        }

        private UpdateStartupContext ParseArgs(string[] args)
        {
            if (args == null || !args.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(args), "args must be specified");
            }

            var startupContext = new UpdateStartupContext
                                 {
                                     ProcessId = ParseProcessId(args[0])
                                 };

            if (OsInfo.IsNotWindows)
            {
                switch (args.Count())
                {
                    case 1:
                        return startupContext;
                    default:
                        {
                            Logger.Debug("Arguments:");

                            foreach (var arg in args)
                            {
                                Logger.Debug("  {0}", arg);
                            }

                            startupContext.UpdateLocation = args[1];
                            startupContext.ExecutingApplication = args[2];

                            break;
                        }
                }
            }

            return startupContext;
        }

        private int ParseProcessId(string arg)
        {
            int id;
            if (!int.TryParse(arg, out id) || id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arg), "Invalid process ID");
            }

            Logger.Debug("NzbDrone process ID: {0}", id);
            return id;
        }

        private string GetInstallationDirectory(UpdateStartupContext startupContext)
        {
            if (startupContext.ExecutingApplication.IsNullOrWhiteSpace())
            {
                Logger.Debug("Using process ID to find installation directory: {0}", startupContext.ProcessId);
                var exeFileInfo = new FileInfo(_processProvider.GetProcessById(startupContext.ProcessId).StartPath);
                Logger.Debug("Executable location: {0}", exeFileInfo.FullName);

                return exeFileInfo.DirectoryName;
            }
            else
            {
                Logger.Debug("Using executing application: {0}", startupContext.ExecutingApplication);
                var exeFileInfo = new FileInfo(startupContext.ExecutingApplication);
                Logger.Debug("Executable location: {0}", exeFileInfo.FullName);

                return exeFileInfo.DirectoryName;
            }
        }
    }
}
