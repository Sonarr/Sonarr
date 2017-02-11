using System.Linq;
using NzbDrone.Core.Instrumentation;
using Sonarr.Http;

namespace NzbDrone.Api.Logs
{
    public class LogModule : SonarrRestModule<LogResource>
    {
        private readonly ILogService _logService;

        public LogModule(ILogService logService)
        {
            _logService = logService;
            GetResourcePaged = GetLogs;
        }

        private PagingResource<LogResource> GetLogs(PagingResource<LogResource> pagingResource)
        {
            var pageSpec = pagingResource.MapToPagingSpec<LogResource, Log>();

            if (pageSpec.SortKey == "time")
            {
                pageSpec.SortKey = "id";
            }

            var filter = pagingResource.Filters.FirstOrDefault();

            if (filter != null && filter.Key == "level")
            {
                switch (filter.Value)
                {
                    case "Fatal":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal");
                        break;
                    case "Error":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error");
                        break;
                    case "Warn":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error" || h.Level == "Warn");
                        break;
                    case "Info":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error" || h.Level == "Warn" || h.Level == "Info");
                        break;
                    case "Debug":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error" || h.Level == "Warn" || h.Level == "Info" || h.Level == "Debug");
                        break;
                    case "Trace":
                        pageSpec.FilterExpressions.Add(h => h.Level == "Fatal" || h.Level == "Error" || h.Level == "Warn" || h.Level == "Info" || h.Level == "Debug" || h.Level == "Trace");
                        break;
                }
            }

            return ApplyToPage(_logService.Paged, pageSpec, LogResourceMapper.ToResource);
        }
    }
}
