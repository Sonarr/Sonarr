using System.Threading.Tasks;
using Nancy.Routing;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;

namespace Sonarr.Api.V3.System
{
    public class SystemModule : SonarrV3Module
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IPlatformInfo _platformInfo;
        private readonly IOsInfo _osInfo;
        private readonly IRouteCacheProvider _routeCacheProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IMainDatabase _database;
        private readonly ILifecycleService _lifecycleService;
        private readonly IDeploymentInfoProvider _deploymentInfoProvider;

        public SystemModule(IAppFolderInfo appFolderInfo,
                            IRuntimeInfo runtimeInfo,
                            IPlatformInfo platformInfo,
                            IOsInfo osInfo,
                            IRouteCacheProvider routeCacheProvider,
                            IConfigFileProvider configFileProvider,
                            IMainDatabase database,
                            ILifecycleService lifecycleService,
                            IDeploymentInfoProvider deploymentInfoProvider)
            : base("system")
        {
            _appFolderInfo = appFolderInfo;
            _runtimeInfo = runtimeInfo;
            _platformInfo = platformInfo;
            _osInfo = osInfo;
            _routeCacheProvider = routeCacheProvider;
            _configFileProvider = configFileProvider;
            _database = database;
            _lifecycleService = lifecycleService;
            _deploymentInfoProvider = deploymentInfoProvider;
            Get("/status",  x => GetStatus());
            Get("/routes",  x => GetRoutes());
            Post("/shutdown",  x => Shutdown());
            Post("/restart",  x => Restart());
        }

        private object GetStatus()
        {
            return new
                   {
                       AppName = BuildInfo.AppName,
                       Version = BuildInfo.Version.ToString(),
                       BuildTime = BuildInfo.BuildDateTime,
                       IsDebug = BuildInfo.IsDebug,
                       IsProduction = RuntimeInfo.IsProduction,
                       IsAdmin = _runtimeInfo.IsAdmin,
                       IsUserInteractive = RuntimeInfo.IsUserInteractive,
                       StartupPath = _appFolderInfo.StartUpFolder,
                       AppData = _appFolderInfo.GetAppDataPath(),
                       OsName = _osInfo.Name,
                       OsVersion = _osInfo.Version,
                       IsNetCore = PlatformInfo.IsNetCore,
                       IsLinux = OsInfo.IsLinux,
                       IsOsx = OsInfo.IsOsx,
                       IsWindows = OsInfo.IsWindows,
                       IsDocker = _osInfo.IsDocker,
                       Mode = _runtimeInfo.Mode,
                       Branch = _configFileProvider.Branch,
                       Authentication = _configFileProvider.AuthenticationMethod,
                       SqliteVersion = _database.Version,
                       UrlBase = _configFileProvider.UrlBase,
                       RuntimeVersion = _platformInfo.Version,
                       RuntimeName = PlatformInfo.Platform,
                       StartTime = _runtimeInfo.StartTime,
                       PackageVersion = _deploymentInfoProvider.PackageVersion,
                       PackageAuthor = _deploymentInfoProvider.PackageAuthor,
                       PackageUpdateMechanism = _deploymentInfoProvider.PackageUpdateMechanism,
                       PackageUpdateMechanismMessage = _deploymentInfoProvider.PackageUpdateMechanismMessage
                   };
        }

        private object GetRoutes()
        {
            return _routeCacheProvider.GetCache().Values;
        }

        private object Shutdown()
        {
            Task.Factory.StartNew(() => _lifecycleService.Shutdown());
            return new { ShuttingDown = true };
        }

        private object Restart()
        {
            Task.Factory.StartNew(() => _lifecycleService.Restart());
            return new { Restarting = true };
        }
    }
}
