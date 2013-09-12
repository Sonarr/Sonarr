using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation.Commands;
using NzbDrone.Core.Messaging;

namespace NzbDrone.Core.Instrumentation
{
    public interface ILogService
    {
        PagingSpec<Log> Paged(PagingSpec<Log> pagingSpec);
    }

    public class LogService : ILogService, IExecute<TrimLogCommand>, IExecute<ClearLogCommand>
    {
        private readonly ILogRepository _logRepository;

        public LogService(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public PagingSpec<Log> Paged(PagingSpec<Log> pagingSpec)
        {
            return _logRepository.GetPaged(pagingSpec);
        }

        public void Execute(TrimLogCommand message)
        {
            _logRepository.Trim();
        }

        public void Execute(ClearLogCommand message)
        {
            _logRepository.Purge();
        }
    }
}