using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Processes;

namespace NzbDrone.Host
{
    public interface ISingleInstancePolicy
    {
        void PreventStartIfAlreadyRunning();
        void KillAllOtherInstance();
    }

    public class SingleInstancePolicy : ISingleInstancePolicy
    {
        private readonly IProcessProvider _processProvider;
        private readonly IBrowserService _browserService;
        private readonly INzbDroneProcessProvider _nzbDroneProcessProvider;
        private readonly Logger _logger;

        public SingleInstancePolicy(IProcessProvider processProvider,
                                    IBrowserService browserService,
                                    INzbDroneProcessProvider nzbDroneProcessProvider,
                                    Logger logger)
        {
            _processProvider = processProvider;
            _browserService = browserService;
            _nzbDroneProcessProvider = nzbDroneProcessProvider;
            _logger = logger;
        }

        public void PreventStartIfAlreadyRunning()
        {
            if (IsAlreadyRunning())
            {
                _logger.Warn("Another instance of NzbDrone is already running.");
                _browserService.LaunchWebUI();
                throw new TerminateApplicationException("Another instance is already running");
            }
        }

        public void KillAllOtherInstance()
        {
            foreach (var processId in GetOtherNzbDroneProcessIds())
            {
                _processProvider.Kill(processId);
            }
        }

        private bool IsAlreadyRunning()
        {
            return GetOtherNzbDroneProcessIds().Any();
        }

        private List<int> GetOtherNzbDroneProcessIds()
        {
            var currentId = _processProvider.GetCurrentProcess().Id;
            var otherProcesses = _nzbDroneProcessProvider.FindNzbDroneProcesses()
                                                         .Select(c => c.Id)
                                                         .Except(new[] {currentId})
                                                         .ToList();

            if (otherProcesses.Any())
            {
                _logger.Info("{0} instance(s) of NzbDrone are running", otherProcesses.Count);
            }

            return otherProcesses;
        }
    }
}