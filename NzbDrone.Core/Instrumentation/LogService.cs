using System.Linq;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Core.Instrumentation
{
    public interface ILogService
    {
        void DeleteAll();
        void Trim();
    }

    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;
        private readonly DiskProvider _diskProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly Logger _logger;

        public LogService(ILogRepository logRepository, DiskProvider diskProvider,
                          EnvironmentProvider environmentProvider, Logger logger)
        {
            _logRepository = logRepository;
            _diskProvider = diskProvider;
            _environmentProvider = environmentProvider;
            _logger = logger;
        }

        public void DeleteAll()
        {
            _logRepository.Purge();
        }

        public void Trim()
        {
            _logRepository.Trim();
        }
    }
}