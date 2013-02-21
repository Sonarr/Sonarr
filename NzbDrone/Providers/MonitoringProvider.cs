using System;
using System.Diagnostics;
using System.Threading;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Providers
{
    public class MonitoringProvider
    {
        private static readonly Logger logger = LogManager.GetLogger("Host.MonitoringProvider");

        private readonly ProcessProvider _processProvider;

        private Timer _processPriorityCheckTimer;

        public MonitoringProvider(ProcessProvider processProvider)
        {
            _processProvider = processProvider;
        }

        public void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e.ExceptionObject as Exception));


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
                logger.WarnException("Unable to verify priority", e);
            }
        }

        public static void AppDomainException(Exception excepion)
        {
            Console.WriteLine("EPIC FAIL: {0}", excepion);
            logger.FatalException("EPIC FAIL: " + excepion.Message, excepion);
        }
    }
}