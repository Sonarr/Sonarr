using System;
using System.Reflection;
using System.Threading;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Instrumentation;

namespace NzbDrone.Host
{
    public static class Bootstrap
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(Bootstrap));
        private static IContainer _container;

        public static void Start(StartupContext startupContext, IUserAlert userAlert, Action<IContainer> startCallback = null)
        {
            try
            {
                Logger.Info("Starting Sonarr - {0} - Version {1}", Assembly.GetCallingAssembly().Location, Assembly.GetExecutingAssembly().GetName().Version);

                if (!PlatformValidation.IsValidate(userAlert))
                {
                    throw new TerminateApplicationException("Missing system requirements");
                }

                LongPathSupport.Enable();

                _container = MainAppContainerBuilder.BuildContainer(startupContext);
                _container.Resolve<InitializeLogger>().Initialize();
                _container.Resolve<IAppFolderFactory>().Register();
                _container.Resolve<IProvidePidFile>().Write();

                var appMode = GetApplicationMode(startupContext);

                Start(appMode, startupContext);

                if (startCallback != null)
                {
                    startCallback(_container);
                }
                else
                {
                    SpinToExit(appMode);
                }
            }
            catch (InvalidConfigFileException ex)
            {
                throw new SonarrStartupException(ex);
            }
            catch (TerminateApplicationException ex)
            {
                Logger.Info(ex.Message);
                LogManager.Configuration = null;
            }
        }

        private static void Start(ApplicationModes applicationModes, StartupContext startupContext)
        {
            _container.Resolve<ReconfigureLogging>().Reconfigure();

            if (!IsInUtilityMode(applicationModes))
            {
                if (startupContext.Flags.Contains(StartupContext.RESTART))
                {
                    Thread.Sleep(2000);
                }

                EnsureSingleInstance(applicationModes == ApplicationModes.Service, startupContext);
            }

            _container.Resolve<Router>().Route(applicationModes);
        }

        private static void SpinToExit(ApplicationModes applicationModes)
        {
            if (IsInUtilityMode(applicationModes))
            {
                return;
            }

            _container.Resolve<IWaitForExit>().Spin();
        }

        private static void EnsureSingleInstance(bool isService, IStartupContext startupContext)
        {
            var instancePolicy = _container.Resolve<ISingleInstancePolicy>();

            if (startupContext.Flags.Contains(StartupContext.TERMINATE))
            {
                instancePolicy.KillAllOtherInstance();
            }
            else if (startupContext.Args.ContainsKey(StartupContext.APPDATA))
            {
                instancePolicy.WarnIfAlreadyRunning();
            }
            else if (isService)
            {
                instancePolicy.KillAllOtherInstance();
            }
            else
            {
                instancePolicy.PreventStartIfAlreadyRunning();
            }
        }

        private static ApplicationModes GetApplicationMode(IStartupContext startupContext)
        {
            if (startupContext.Help)
            {
                return ApplicationModes.Help;
            }

            if (OsInfo.IsWindows && startupContext.RegisterUrl)
            {
                return ApplicationModes.RegisterUrl;
            }

            if (OsInfo.IsWindows && startupContext.InstallService)
            {
                return ApplicationModes.InstallService;
            }

            if (OsInfo.IsWindows && startupContext.UninstallService)
            {
                return ApplicationModes.UninstallService;
            }

            if (_container.Resolve<IRuntimeInfo>().IsWindowsService)
            {
                return ApplicationModes.Service;
            }

            return ApplicationModes.Interactive;
        }

        private static bool IsInUtilityMode(ApplicationModes applicationMode)
        {
            switch (applicationMode)
            {
                case ApplicationModes.InstallService:
                case ApplicationModes.UninstallService:
                case ApplicationModes.RegisterUrl:
                case ApplicationModes.Help:
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }
    }
}
