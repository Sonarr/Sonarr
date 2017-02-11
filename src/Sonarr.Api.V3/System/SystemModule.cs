using Nancy;
using Nancy.Routing;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;
using Sonarr.Http.Extensions;

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

        public SystemModule(IAppFolderInfo appFolderInfo,
                            IRuntimeInfo runtimeInfo,
                            IPlatformInfo platformInfo,
                            IOsInfo osInfo,
                            IRouteCacheProvider routeCacheProvider,
                            IConfigFileProvider configFileProvider,
                            IMainDatabase database,
                            ILifecycleService lifecycleService)
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
            Get["/status"] = x => GetStatus();
            Get["/routes"] = x => GetRoutes();
            Post["/shutdown"] = x => Shutdown();
            Post["/restart"] = x => Restart();
        }

        private Response GetStatus()
        {
            return new
                   {
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
                       IsMonoRuntime = PlatformInfo.IsMono,
                       IsMono = PlatformInfo.IsMono,
                       IsLinux = OsInfo.IsLinux,
                       IsOsx = OsInfo.IsOsx,
                       IsWindows = OsInfo.IsWindows,
                       Mode = _runtimeInfo.Mode,
                       Branch = _configFileProvider.Branch,
                       Authentication = _configFileProvider.AuthenticationMethod,
                       SqliteVersion = _database.Version,
                       UrlBase = _configFileProvider.UrlBase,
                       RuntimeVersion = _platformInfo.Version,
                       RuntimeName = PlatformInfo.Platform,
                       StartTime = _runtimeInfo.StartTime
                   }.AsResponse();
        }

        private Response GetRoutes()
        {
            return _routeCacheProvider.GetCache().Values.AsResponse();
        }

        private Response Shutdown()
        {
            _lifecycleService.Shutdown();
            return "".AsResponse();
        }

        private Response Restart()
        {
            _lifecycleService.Restart();
            return "".AsResponse();
        }
    }
}
