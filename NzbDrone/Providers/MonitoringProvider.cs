using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Timers;
using NLog;
using Ninject;

namespace NzbDrone.Providers
{
    public class MonitoringProvider
    {
        private static readonly Logger Logger = LogManager.GetLogger("MonitoringProvider");

        private readonly IISProvider _iisProvider;
        private readonly ProcessProvider _processProvider;

        private int _pingFailCounter;
        private Timer _pingTimer;

        [Inject]
        public MonitoringProvider(ProcessProvider processProvider, IISProvider iisProvider)
        {
            _processProvider = processProvider;
            _iisProvider = iisProvider;
        }

        public void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e));

            AppDomain.CurrentDomain.ProcessExit += ProgramExited;
            AppDomain.CurrentDomain.DomainUnload += ProgramExited;

            var prioCheckTimer = new Timer(5000);
            prioCheckTimer.Elapsed += EnsurePriority;
            prioCheckTimer.Enabled = true;

            _pingTimer = new Timer(60000) { AutoReset = true };
            _pingTimer.Elapsed += (PingServer);
            _pingTimer.Start();
        }

        public MonitoringProvider()
        {
        }


        public virtual void EnsurePriority(object sender, ElapsedEventArgs e)
        {
            var currentProcess = _processProvider.GetCurrentProcess();
            if (currentProcess.Priority != ProcessPriorityClass.Normal)
            {
                _processProvider.SetPriority(currentProcess.Id, ProcessPriorityClass.Normal);
            }

            var iisProcess = _processProvider.GetProcessById(_iisProvider.IISProcessId);
            if (iisProcess != null && iisProcess.Priority != ProcessPriorityClass.Normal &&
                iisProcess.Priority != ProcessPriorityClass.AboveNormal)
            {
                _processProvider.SetPriority(iisProcess.Id, ProcessPriorityClass.Normal);
            }
        }

        public virtual void PingServer(object sender, ElapsedEventArgs e)
        {
            if (!_iisProvider.ServerStarted) return;

            try
            {
                string response = new WebClient().DownloadString(_iisProvider.AppUrl + "/health");

                if (!response.Contains("OK"))
                {
                    throw new ServerException("Health services responded with an invalid response.");
                }
                if (_pingFailCounter > 0)
                {
                    Logger.Info("Application pool has been successfully recovered.");
                }
                _pingFailCounter = 0;
            }
            catch (Exception ex)
            {
                _pingFailCounter++;
                Logger.ErrorException("Application pool is not responding. Count " + _pingFailCounter, ex);
                if (_pingFailCounter > 2)
                {
                    _iisProvider.RestartServer();
                }
            }
        }

        private void ProgramExited(object sender, EventArgs e)
        {
            _iisProvider.StopServer();
        }


        private static void AppDomainException(object excepion)
        {
            Console.WriteLine("EPIC FAIL: {0}", excepion);
            Logger.Fatal("EPIC FAIL: {0}", excepion);

#if RELEASE
            new Client
            {
                ApiKey = "43BBF60A-EB2A-4C1C-B09E-422ADF637265",
                ApplicationName = "NZBDrone",
                CurrentException = excepion as Exception
            }.Submit();
#endif
        }
    }
}