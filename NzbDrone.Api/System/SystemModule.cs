using Nancy;
using Nancy.Routing;
using NzbDrone.Common;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.System
{
    public class SystemModule : NzbDroneApiModule
    {
        private readonly IAppDirectoryInfo _appDirectoryInfo;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IRouteCacheProvider _routeCacheProvider;

        public SystemModule(IAppDirectoryInfo appDirectoryInfo, IRuntimeInfo runtimeInfo, IRouteCacheProvider routeCacheProvider)
            : base("system")
        {
            _appDirectoryInfo = appDirectoryInfo;
            _runtimeInfo = runtimeInfo;
            _routeCacheProvider = routeCacheProvider;
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
                    StartupPath = _appDirectoryInfo.StartUpPath,
                    AppData = _appDirectoryInfo.GetAppDataPath(),
                    OsVersion = OsInfo.Version.ToString(),
                    IsMono = OsInfo.IsMono,
                    IsLinux = OsInfo.IsLinux,
                }.AsResponse();

        }


        private Response GetRoutes()
        {
            return _routeCacheProvider.GetCache().Values.AsResponse();
        }
    }
}
