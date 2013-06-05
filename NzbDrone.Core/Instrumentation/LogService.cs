using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Instrumentation
{
    public interface ILogService
    {
        void DeleteAll();
        void Trim();
        PagingSpec<Log> Paged(PagingSpec<Log> pagingSpec);
    }

    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;

        public LogService(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public void DeleteAll()
        {
            _logRepository.Purge();
        }

        public void Trim()
        {
            _logRepository.Trim();
        }

        public PagingSpec<Log> Paged(PagingSpec<Log> pagingSpec)
        {
            return _logRepository.GetPaged(pagingSpec);
        }
    }
}