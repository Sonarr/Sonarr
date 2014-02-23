using Nancy;
using Nancy.Routing;
using NzbDrone.Common;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Api.System
{
    public class SystemModule : NzbDroneApiModule
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IRouteCacheProvider _routeCacheProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IDatabase _database;

        public SystemModule(IAppFolderInfo appFolderInfo, IRuntimeInfo runtimeInfo, IRouteCacheProvider routeCacheProvider, IConfigFileProvider configFileProvider, IDatabase database)
            : base("system")
        {
            _appFolderInfo = appFolderInfo;
            _runtimeInfo = runtimeInfo;
            _routeCacheProvider = routeCacheProvider;
            _configFileProvider = configFileProvider;
            _database = database;
            Get["/status"] = x => GetStatus();
            Get["/routes"] = x => GetRoutes();
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
                    IsUserInteractive = _runtimeInfo.IsUserInteractive,
                    StartupPath = _appFolderInfo.StartUpFolder,
                    AppData = _appFolderInfo.GetAppDataPath(),
                    OsVersion = OsInfo.Version.ToString(),
                    IsMono = OsInfo.IsMono,
                    IsLinux = OsInfo.IsLinux,
                    IsWindows = OsInfo.IsWindows,
                    Branch = _configFileProvider.Branch,
                    Authentication = _configFileProvider.AuthenticationEnabled,
                    StartOfWeek = (int)OsInfo.FirstDayOfWeek,
                    SqliteVersion = _database.Version,
                    UrlBase = _configFileProvider.UrlBase
                }.AsResponse();

        }


        private Response GetRoutes()
        {
            return _routeCacheProvider.GetCache().Values.AsResponse();
        }
    }
}
