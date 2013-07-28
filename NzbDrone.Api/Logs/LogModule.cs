using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Logs
{
    public class LogModule : NzbDroneRestModule<LogResource>
    {
        private readonly ILogService _logService;

        public LogModule(ILogService logService)
        {
            _logService = logService;
            GetResourcePaged = GetLogs;
        }

        private PagingResource<LogResource> GetLogs(PagingResource<LogResource> pagingResource)
        {
            var pageSpec = pagingResource.InjectTo<PagingSpec<Log>>();
            return ApplyToPage(_logService.Paged, pageSpec);
        }
    }
}