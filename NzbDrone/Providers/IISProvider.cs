using System;
using System.Diagnostics;
using NLog;
using Ninject;
using NzbDrone.Common;

namespace NzbDrone.Providers
{
    public class IISProvider
    {
        private static readonly Logger IISLogger = LogManager.GetLogger("Host.IISExpress");
        private static readonly Logger Logger = LogManager.GetLogger("Host.IISProvider");
        private readonly ConfigFileProvider _configFileProvider;
        private readonly ProcessProvider _processProvider;
        private readonly EnviromentProvider _enviromentProvider;


        [Inject]
        public IISProvider(ConfigFileProvider configFileProvider, ProcessProvider processProvider, EnviromentProvider enviromentProvider)
        {
            _configFileProvider = configFileProvider;
            _processProvider = processProvider;
            _enviromentProvider = enviromentProvider;
        }

        public IISProvider()
        {
        }

        public string AppUrl
        {
            get { return string.Format("http://localhost:{0}/", _configFileProvider.Port); }
        }

        public int IISProcessId { get; private set; }

        public bool ServerStarted { get; private set; }

        public void StartServer()
        {
            Logger.Info("Preparing IISExpress Server...");

            var startInfo = new ProcessStartInfo();

            startInfo.FileName = _enviromentProvider.GetIISExe();
            startInfo.Arguments = String.Format("/config:\"{0}\" /trace:i", _enviromentProvider.GetIISConfigPath());
            startInfo.WorkingDirectory = _enviromentProvider.ApplicationPath;

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;


            startInfo.EnvironmentVariables[EnviromentProvider.NZBDRONE_PATH] = _enviromentProvider.ApplicationPath;
            startInfo.EnvironmentVariables[EnviromentProvider.NZBDRONE_PID] = Process.GetCurrentProcess().Id.ToString();

            try
            {
                _configFileProvider.UpdateIISConfig(_enviromentProvider.GetIISConfigPath());
            }
            catch (Exception e)
            {
                Logger.ErrorException("An error has occurred while trying to update the config file.", e);
            }

            var iisProcess = _processProvider.Start(startInfo);
            IISProcessId = iisProcess.Id;

            iisProcess.OutputDataReceived += (OnOutputDataReceived);
            iisProcess.ErrorDataReceived += (OnErrorDataReceived);

            iisProcess.BeginErrorReadLine();
            iisProcess.BeginOutputReadLine();

            ServerStarted = true;
        }

        private static void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e == null || String.IsNullOrWhiteSpace(e.Data))
                return;

            IISLogger.Error(e.Data);
        }

        public void StopServer()
        {
            _processProvider.Kill(IISProcessId);

            Logger.Info("Finding orphaned IIS Processes.");
            foreach (var process in _processProvider.GetProcessByName("IISExpress"))
            {
                Logger.Info("[{0}]IIS Process found. Path:{1}", process.Id, process.StartPath);
                if (process.StartPath.NormalizePath() == _enviromentProvider.GetIISExe().NormalizePath())
                {
                    Logger.Info("[{0}]Process is considered orphaned.", process.Id);
                    _processProvider.Kill(process.Id);
                }
                else
                {
                    Logger.Info("[{0}]Process has a different start-up path. skipping.", process.Id);
                }
            }
        }

        public void RestartServer()
        {
            ServerStarted = false;
            Logger.Warn("Attempting to restart server.");
            StopServer();
            StartServer();
        }

        private void OnOutputDataReceived(object s, DataReceivedEventArgs e)
        {
            if (e == null || String.IsNullOrWhiteSpace(e.Data) || e.Data.StartsWith("Request started:") ||
                e.Data.StartsWith("Request ended:") || e.Data == ("IncrementMessages called") || e.Data == "iisexpress")
                return;

            //if (e.Data.Contains(" NzbDrone."))
            {
                Console.WriteLine(e.Data);
                return;
            }

            IISLogger.Trace(e.Data);
        }

    }
}