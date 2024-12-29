using Workarr.Configuration;
using Workarr.Instrumentation;

namespace Workarr.Housekeeping.Housekeepers
{
    public class TrimLogDatabase : IHousekeepingTask
    {
        private readonly ILogRepository _logRepo;
        private readonly IConfigFileProvider _configFileProvider;

        public TrimLogDatabase(ILogRepository logRepo, IConfigFileProvider configFileProvider)
        {
            _logRepo = logRepo;
            _configFileProvider = configFileProvider;
        }

        public void Clean()
        {
            if (!_configFileProvider.LogDbEnabled)
            {
                return;
            }

            _logRepo.Trim();
        }
    }
}
