using System.Linq;
using NLog;
using NzbDrone.Common.Processes;

namespace NzbDrone.Host
{
    public interface ISingleInstancePolicy
    {
        void EnforceSingleInstance();
    }

    public class SingleInstancePolicy : ISingleInstancePolicy
    {
        private readonly IProcessProvider _processProvider;
        private readonly IBrowserService _browserService;
        private readonly Logger _logger;

        public SingleInstancePolicy(IProcessProvider processProvider, IBrowserService browserService, Logger logger)
        {
            _processProvider = processProvider;
            _browserService = browserService;
            _logger = logger;
        }

        public void EnforceSingleInstance()
        {
            if (IsAlreadyRunning())
            {
                _logger.Warn("Another instance of NzbDrone is already running.");
                _browserService.LaunchWebUI();
                throw new TerminateApplicationException("Another instance is already running");
            }
        }

        private bool IsAlreadyRunning()
        {
            var currentId = _processProvider.GetCurrentProcess().Id;
            var consoleIds = _processProvider.FindProcessByName(ProcessProvider.NZB_DRONE_CONSOLE_PROCESS_NAME).Select(c => c.Id);
            var guiIds = _processProvider.FindProcessByName(ProcessProvider.NZB_DRONE_PROCESS_NAME).Select(c => c.Id);

            var otherProcesses = consoleIds.Union(guiIds).Except(new[] { currentId });

            return otherProcesses.Any();
        }

    }
}