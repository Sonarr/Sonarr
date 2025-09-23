using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;
using Sonarr.Http;
using Sonarr.Http.Validation;

namespace Sonarr.Api.V3.System
{
    [V3ApiController]
    public class SystemController : Controller
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IPlatformInfo _platformInfo;
        private readonly IOsInfo _osInfo;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IMainDatabase _database;
        private readonly ILifecycleService _lifecycleService;
        private readonly IDeploymentInfoProvider _deploymentInfoProvider;
        private readonly EndpointDataSource _endpointData;
        private readonly DfaGraphWriter _graphWriter;
        private readonly DuplicateEndpointDetector _detector;

        public SystemController(IAppFolderInfo appFolderInfo,
                                IRuntimeInfo runtimeInfo,
                                IPlatformInfo platformInfo,
                                IOsInfo osInfo,
                                IConfigFileProvider configFileProvider,
                                IMainDatabase database,
                                ILifecycleService lifecycleService,
                                IDeploymentInfoProvider deploymentInfoProvider,
                                EndpointDataSource endpoints,
                                DfaGraphWriter graphWriter,
                                DuplicateEndpointDetector detector)
        {
            _appFolderInfo = appFolderInfo;
            _runtimeInfo = runtimeInfo;
            _platformInfo = platformInfo;
            _osInfo = osInfo;
            _configFileProvider = configFileProvider;
            _database = database;
            _lifecycleService = lifecycleService;
            _deploymentInfoProvider = deploymentInfoProvider;
            _endpointData = endpoints;
            _graphWriter = graphWriter;
            _detector = detector;
        }

        [HttpGet("status")]
        [Produces("application/json")]
        public SystemResource GetStatus()
        {
            return new SystemResource
            {
                AppName = BuildInfo.AppName,
                InstanceName = _configFileProvider.InstanceName,
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
                IsNetCore = true,
                IsLinux = OsInfo.IsLinux,
                IsOsx = OsInfo.IsOsx,
                IsWindows = OsInfo.IsWindows,
                IsDocker = _osInfo.IsDocker,
                IsContainerized = _osInfo.IsContainerized,
                Mode = _runtimeInfo.Mode,
                Branch = _configFileProvider.Branch,
                Authentication = _configFileProvider.AuthenticationMethod,
                DatabaseType = _database.DatabaseType,
                DatabaseVersion = _database.Version,
                MigrationVersion = _database.Migration,
                UrlBase = _configFileProvider.UrlBase,
                RuntimeVersion = _platformInfo.Version,
                RuntimeName = PlatformInfo.PlatformName,
                StartTime = _runtimeInfo.StartTime,
                PackageVersion = _deploymentInfoProvider.PackageVersion,
                PackageAuthor = _deploymentInfoProvider.PackageAuthor,
                PackageUpdateMechanism = _deploymentInfoProvider.PackageUpdateMechanism,
                PackageUpdateMechanismMessage = _deploymentInfoProvider.PackageUpdateMechanismMessage
            };
        }

        [HttpGet("routes")]
        [Produces("application/json")]
        public IActionResult GetRoutes()
        {
            using (var sw = new StringWriter())
            {
                _graphWriter.Write(_endpointData, sw);
                var graph = sw.ToString();
                return Content(graph, "text/plain");
            }
        }

        [HttpGet("routes/duplicate")]
        [Produces("application/json")]
        public object DuplicateRoutes()
        {
            return _detector.GetDuplicateEndpoints(_endpointData);
        }

        [HttpPost("shutdown")]
        public object Shutdown()
        {
            Task.Factory.StartNew(() => _lifecycleService.Shutdown());
            return new { ShuttingDown = true };
        }

        [HttpPost("restart")]
        public object Restart()
        {
            Task.Factory.StartNew(() => _lifecycleService.Restart());
            return new { Restarting = true };
        }
    }
}
