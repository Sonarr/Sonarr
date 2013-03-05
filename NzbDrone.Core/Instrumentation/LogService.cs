using System.Linq;

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
    }
}