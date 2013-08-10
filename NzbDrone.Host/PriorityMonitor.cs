using System;
using System.Diagnostics;
using System.Threading;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Host
{
    public class PriorityMonitor
    {
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        private Timer _processPriorityCheckTimer;

        public PriorityMonitor(IProcessProvider processProvider, Logger logger)
        {
            _processProvider = processProvider;
            _logger = logger;
        }

        public void Start()
        {
            _processPriorityCheckTimer = new Timer(EnsurePriority);
            _processPriorityCheckTimer.Change(TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(30));

        }

        public virtual void EnsurePriority(object sender)
        {
            try
            {
                if (_processProvider.GetCurrentProcessPriority() != ProcessPriorityClass.Normal)
                {
                    _processProvider.SetPriority(_processProvider.GetCurrentProcess().Id, ProcessPriorityClass.Normal);
                }
            }
            catch (Exception e)
            {
                _logger.WarnException("Unable to verify priority", e);
            }
        }
    }
}