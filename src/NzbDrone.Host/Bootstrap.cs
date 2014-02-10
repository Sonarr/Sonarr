using System;
using System.Reflection;
using System.Threading;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Security;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Host
{
    public static class Bootstrap
    {
        private static IContainer _container;
        private static readonly Logger Logger = NzbDroneLogger.GetLogger();

        public static void Start(StartupContext startupContext, IUserAlert userAlert, Action<IContainer> startCallback = null)
        {
            LogTargets.Register(startupContext, false, true);

            try
            {
                GlobalExceptionHandlers.Register();
                IgnoreCertErrorPolicy.Register();

                Logger.Info("Starting NzbDrone Console. Version {0}", Assembly.GetExecutingAssembly().GetName().Version);

                if (!PlatformValidation.IsValidate(userAlert))
                {
                    throw new TerminateApplicationException("Missing system requirements");
                }

                _container = MainAppContainerBuilder.BuildContainer(startupContext);
                _container.Resolve<IAppFolderFactory>().Register();

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
            catch (TerminateApplicationException e)
            {
                Logger.Info(e.Message);
                LogManager.Configuration = null;
            }
        }

        private static void Start(ApplicationModes applicationModes, StartupContext startupContext)
        {
            if (!IsInUtilityMode(applicationModes))
            {
                EnsureSingleInstance(applicationModes == ApplicationModes.Service, startupContext);
            }

            DbFactory.RegisterDatabase(_container);
            _container.Resolve<Router>().Route(applicationModes);
        }

        private static void SpinToExit(ApplicationModes applicationModes)
        {
            if (IsInUtilityMode(applicationModes))
            {
                return;
            }

            var runTimeInfo = _container.Resolve<IRuntimeInfo>();

            while (runTimeInfo.IsRunning)
            {
                Thread.Sleep(1000);
            }
        }

        private static void EnsureSingleInstance(bool isService, StartupContext startupContext)
        {
            var instancePolicy = _container.Resolve<ISingleInstancePolicy>();

            if (isService)
            {
                instancePolicy.KillAllOtherInstance();
            }
            else if (startupContext.Flags.Contains(StartupContext.TERMINATE))
            {
                instancePolicy.KillAllOtherInstance();
            }
            else
            {
                instancePolicy.PreventStartIfAlreadyRunning();
            }
        }

        private static ApplicationModes GetApplicationMode(StartupContext startupContext)
        {
            if (startupContext.Flags.Contains(StartupContext.HELP))
            {
                return ApplicationModes.Help;
            }

            if (!OsInfo.IsLinux && startupContext.InstallService)
            {
                return ApplicationModes.InstallService;
            }

            if (!OsInfo.IsLinux && startupContext.UninstallService)
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