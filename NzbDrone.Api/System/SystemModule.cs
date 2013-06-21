using Nancy;
using Nancy.Routing;
using NzbDrone.Common;
using NzbDrone.Api.Extensions;
using System.Linq;

namespace NzbDrone.Api.System
{
    public class SystemModule : NzbDroneApiModule
    {
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly IRouteCacheProvider _routeCacheProvider;

        public SystemModule(IEnvironmentProvider environmentProvider, IRouteCacheProvider routeCacheProvider)
            : base("system")
        {
            _environmentProvider = environmentProvider;
            _routeCacheProvider = routeCacheProvider;
            Get["/status"] = x => GetStatus();
            Get["/routes"] = x => GetRoutes();
        }

        private Response GetStatus()
        {
            return new
                {
                    Version = _environmentProvider.Version.ToString(),
                    AppData = _environmentProvider.GetAppDataPath(),
                    IsAdmin = _environmentProvider.IsAdmin,
                    IsUserInteractive = _environmentProvider.IsUserInteractive,
                    BuildTime = _environmentProvider.BuildDateTime,
                    StartupPath = _environmentProvider.StartUpPath,
                    OsVersion = _environmentProvider.GetOsVersion().ToString(),
                    IsMono = EnvironmentProvider.IsMono,
                    IsProduction = EnvironmentProvider.IsProduction,
                    IsDebug = EnvironmentProvider.IsDebug,
                    IsLinux = EnvironmentProvider.IsLinux,
                }.AsResponse();

        }


        private Response GetRoutes()
        {
            return _routeCacheProvider.GetCache().Values.AsResponse();
        }
    }
}
