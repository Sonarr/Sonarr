using Nancy;
using NzbDrone.Common;
using NzbDrone.Api.Extensions;

namespace NzbDrone.Api.System
{
    public class SystemModule : NzbDroneApiModule
    {
        private readonly IEnvironmentProvider _environmentProvider;

        public SystemModule(IEnvironmentProvider environmentProvider)
            : base("system")
        {
            _environmentProvider = environmentProvider;
            Get["/status"] = x => GetStatus();
        }

        private Response GetStatus()
        {
            return new
                {
                    Version = _environmentProvider.Version,
                    AppData = _environmentProvider.GetAppDataPath(),
                    IsAdmin = _environmentProvider.IsAdmin,
                    IsUserInteractive = _environmentProvider.IsUserInteractive,
                    BuildTime = _environmentProvider.BuildDateTime,
                    StartupPath = _environmentProvider.StartUpPath,
                    OsVersion = _environmentProvider.GetOsVersion(),
                    IsMono = EnvironmentProvider.IsMono,
                    IsProduction = EnvironmentProvider.IsProduction,
                    IsDebug = EnvironmentProvider.IsDebug,
                    IsLinux = EnvironmentProvider.IsLinux
                }.AsResponse();

        }
    }
}
