using System;
using System.Diagnostics;
using System.Threading;
using NLog;
using NzbDrone.Common;

namespace NzbDrone
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
                var currentProcess = _processProvider.GetCurrentProcess();
                if (currentProcess.Priority != ProcessPriorityClass.Normal)
                {
                    _processProvider.SetPriority(currentProcess.Id, ProcessPriorityClass.Normal);
                }

                var iisProcess = _processProvider.GetProcessById(_processProvider.GetCurrentProcess().Id);
                if (iisProcess != null && iisProcess.Priority != ProcessPriorityClass.Normal &&
                    iisProcess.Priority != ProcessPriorityClass.AboveNormal)
                {
                    _processProvider.SetPriority(iisProcess.Id, ProcessPriorityClass.Normal);
                }
            }
            catch (Exception e)
            {
                _logger.WarnException("Unable to verify priority", e);
            }
        }
    }
}