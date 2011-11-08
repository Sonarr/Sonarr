using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Timers;
using Exceptioneer.WindowsFormsClient;
using NLog;
using Ninject;
using NzbDrone.Common;

namespace NzbDrone.Providers
{
    public class MonitoringProvider
    {
        private static readonly Logger Logger = LogManager.GetLogger("Host.MonitoringProvider");

        private readonly IISProvider _iisProvider;
        private readonly ProcessProvider _processProvider;
        private readonly WebClientProvider _webClientProvider;

        private int _pingFailCounter;
        private Timer _pingTimer;

        [Inject]
        public MonitoringProvider(ProcessProvider processProvider, IISProvider iisProvider,
                                  WebClientProvider webClientProvider)
        {
            _processProvider = processProvider;
            _iisProvider = iisProvider;
            _webClientProvider = webClientProvider;
        }

        public MonitoringProvider()
        {
        }

        public void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e.ExceptionObject as Exception));

            AppDomain.CurrentDomain.ProcessExit += ProgramExited;
            AppDomain.CurrentDomain.DomainUnload += ProgramExited;

            var prioCheckTimer = new Timer(5000);
            prioCheckTimer.Elapsed += EnsurePriority;
            prioCheckTimer.Enabled = true;

            _pingTimer = new Timer(60000) { AutoReset = true };
            _pingTimer.Elapsed += (PingServer);
            _pingTimer.Start();
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
                string response = _webClientProvider.DownloadString(_iisProvider.AppUrl + "/health");

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


        public static void AppDomainException(Exception excepion)
        {
            Console.WriteLine("EPIC FAIL: {0}", excepion);

#if DEBUG
#else
            new Client
            {
                ApiKey = "43BBF60A-EB2A-4C1C-B09E-422ADF637265",
                ApplicationName = "NzbDrone",
                CurrentException = excepion as Exception
            }.Submit();
#endif

            Logger.Fatal("EPIC FAIL: {0}", excepion);
        }
    }
}