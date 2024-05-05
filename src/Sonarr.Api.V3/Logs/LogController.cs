using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Instrumentation;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Logs
{
    [V3ApiController]
    public class LogController : Controller
    {
        private readonly ILogService _logService;
        private readonly IConfigFileProvider _configFileProvider;

        public LogController(ILogService logService, IConfigFileProvider configFileProvider)
        {
            _logService = logService;
            _configFileProvider = configFileProvider;
        }

        [HttpGet]
        [Produces("application/json")]
        public PagingResource<LogResource> GetLogs([FromQuery] PagingRequestResource paging, string level)
        {
            if (!_configFileProvider.LogDbEnabled)
            {
                return new PagingResource<LogResource>();
            }

            var pagingResource = new PagingResource<LogResource>(paging);
            var pageSpec = pagingResource.MapToPagingSpec<LogResource, Log>();

            if (pageSpec.SortKey == "time")
            {
                pageSpec.SortKey = "id";
            }

            if (level.IsNotNullOrWhiteSpace())
            {
                switch (level)
                {
                    case "fatal":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal");
                        break;
                    case "error":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error");
                        break;
                    case "warn":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error" || h.Level == "Warn");
                        break;
                    case "info":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error" || h.Level == "Warn" || h.Level == "Info");
                        break;
                    case "debug":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error" || h.Level == "Warn" || h.Level == "Info" || h.Level == "Debug");
                        break;
                    case "trace":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error" || h.Level == "Warn" || h.Level == "Info" || h.Level == "Debug" || h.Level == "Trace");
                        break;
                }
            }

            var response = pageSpec.ApplyToPage(_logService.Paged, LogResourceMapper.ToResource);

            if (pageSpec.SortKey == "id")
            {
                response.SortKey = "time";
            }

            return response;
        }
    }
}
