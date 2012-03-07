using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Threading;
using Exceptioneer.WindowsFormsClient;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Common.Model;

namespace NzbDrone.Providers
{
    public class MonitoringProvider
    {
        private static readonly Logger logger = LogManager.GetLogger("Host.MonitoringProvider");

        private readonly IISProvider _iisProvider;
        private readonly ProcessProvider _processProvider;
        private readonly HttpProvider _httpProvider;
        private readonly ConfigFileProvider _configFileProvider;

        private int _pingFailCounter;
        private Timer _pingTimer;
        private Timer _processPriorityCheckTimer;

        [Inject]
        public MonitoringProvider(ProcessProvider processProvider, IISProvider iisProvider,
                                  HttpProvider httpProvider, ConfigFileProvider configFileProvider)
        {
            _processProvider = processProvider;
            _iisProvider = iisProvider;
            _httpProvider = httpProvider;
            _configFileProvider = configFileProvider;
        }

        public MonitoringProvider()
        {
        }

        public void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e.ExceptionObject as Exception));

            AppDomain.CurrentDomain.ProcessExit += ProgramExited;
            AppDomain.CurrentDomain.DomainUnload += ProgramExited;

            _processPriorityCheckTimer = new Timer(EnsurePriority);
            _processPriorityCheckTimer.Change(TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(30));

            _pingTimer = new Timer(PingServer);
            _pingTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));
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

                var iisProcess = _processProvider.GetProcessById(_iisProvider.IISProcessId);
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

        public virtual void PingServer(object sender)
        {
            if (!_iisProvider.ServerStarted) return;

            try
            {
                ICredentials identity = null;

                if (_configFileProvider.AuthenticationType == AuthenticationType.Windows)
                {
                    identity = CredentialCache.DefaultCredentials;
                }

                _httpProvider.DownloadString(_iisProvider.AppUrl, identity); //This should preload the home page, making the first load faster.
                string response = _httpProvider.DownloadString(_iisProvider.AppUrl + "/health", identity);

                if (!response.Contains("OK"))
                {
                    throw new ServerException("Health services responded with an invalid response.");
                }

                if (_pingFailCounter > 0)
                {
                    logger.Info("Application pool has been successfully recovered.");
                }

                _pingFailCounter = 0;
            }
            catch (Exception ex)
            {
                _pingFailCounter++;
                logger.ErrorException("Application pool is not responding. Count " + _pingFailCounter, ex);
                if (_pingFailCounter > 4)
                {
                    _pingFailCounter = 0;
                    //_iisProvider.RestartServer();
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

            if (EnvironmentProvider.IsProduction)
            {
                new Client
                    {
                        ApiKey = "43BBF60A-EB2A-4C1C-B09E-422ADF637265",
                        ApplicationName = "NzbDrone",
                        CurrentException = excepion as Exception
                    }.Submit();
            }

            logger.FatalException("EPIC FAIL: " + excepion.Message, excepion);
        }
    }
}