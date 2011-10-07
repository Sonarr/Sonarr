using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;
using NLog;

namespace NzbDrone.Providers
{
    internal class IISProvider
    {
        private readonly ConfigProvider _configProvider;
        private static readonly Logger IISLogger = LogManager.GetLogger("IISExpress");
        private static readonly Logger Logger = LogManager.GetLogger("IISProvider");

        private readonly string IISExe;
        private readonly string IISConfigPath;

        private static Timer _pingTimer;
        private static int _pingFailCounter;

        private static Process _iisProcess;


        public IISProvider(ConfigProvider configProvider)
        {
            _configProvider = configProvider;
            IISExe = Path.Combine(_configProvider.IISFolder, @"iisexpress.exe");
            IISConfigPath = Path.Combine(_configProvider.IISFolder, "AppServer", "applicationhost.config");
        }

        internal string AppUrl
        {
            get { return string.Format("http://localhost:{0}/", _configProvider.Port); }
        }

        internal int IISProcessId
        {
            get
            {
                if (_iisProcess == null)
                {
                    throw new InvalidOperationException("IIS Process isn't running yet.");
                }

                return _iisProcess.Id;
            }
        }

        internal Process StartServer()
        {
            Logger.Info("Preparing IISExpress Server...");
            _iisProcess = new Process();

            _iisProcess.StartInfo.FileName = IISExe;
            _iisProcess.StartInfo.Arguments = String.Format("/config:\"{0}\" /trace:i", IISConfigPath);//"/config:"""" /trace:i";
            _iisProcess.StartInfo.WorkingDirectory = _configProvider.ApplicationRoot;

            _iisProcess.StartInfo.UseShellExecute = false;
            _iisProcess.StartInfo.RedirectStandardOutput = true;
            _iisProcess.StartInfo.RedirectStandardError = true;
            _iisProcess.StartInfo.CreateNoWindow = true;


            _iisProcess.OutputDataReceived += (OnOutputDataReceived);
            _iisProcess.ErrorDataReceived += (OnErrorDataReceived);

            //Set Variables for the config file.
            _iisProcess.StartInfo.EnvironmentVariables.Add("NZBDRONE_PATH", _configProvider.ApplicationRoot);
            _iisProcess.StartInfo.EnvironmentVariables.Add("NZBDRONE_PID", Process.GetCurrentProcess().Id.ToString());

            try
            {
                UpdateIISConfig();
            }
            catch (Exception e)
            {
                Logger.ErrorException("An error has occurred while trying to update the config file.", e);
            }


            Logger.Info("Starting process. [{0}]", _iisProcess.StartInfo.FileName);



            _iisProcess.Start();
            _iisProcess.PriorityClass = ProcessPriorityClass.AboveNormal;

            _iisProcess.BeginErrorReadLine();
            _iisProcess.BeginOutputReadLine();

            //Start Ping
            _pingTimer = new Timer(300000) { AutoReset = true };
            _pingTimer.Elapsed += (PingServer);
            _pingTimer.Start();

            return _iisProcess;
        }

        private static void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e == null || String.IsNullOrWhiteSpace(e.Data))
                return;

            IISLogger.Error(e.Data);
        }

        internal void StopServer()
        {
            KillProcess(_iisProcess);

            Logger.Info("Finding orphaned IIS Processes.");
            foreach (var process in Process.GetProcessesByName("IISExpress"))
            {
                string processPath = process.MainModule.FileName;
                Logger.Info("[{0}]IIS Process found. Path:{1}", process.Id, processPath);
                if (NormalizePath(processPath) == NormalizePath(IISExe))
                {
                    Logger.Info("[{0}]Process is considered orphaned.", process.Id);
                    KillProcess(process);
                }
                else
                {
                    Logger.Info("[{0}]Process has a different start-up path. skipping.", process.Id);
                }
            }
        }

        private void RestartServer()
        {
            _pingTimer.Stop();
            Logger.Warn("Attempting to restart server.");
            StopServer();
            StartServer();
        }

        private void PingServer(object sender, ElapsedEventArgs e)
        {
            try
            {
                var response = new WebClient().DownloadString(AppUrl + "/health");

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
                    RestartServer();
                }
            }
        }

        private void OnOutputDataReceived(object s, DataReceivedEventArgs e)
        {
            if (e == null || String.IsNullOrWhiteSpace(e.Data) || e.Data.StartsWith("Request started:") ||
                e.Data.StartsWith("Request ended:") || e.Data == ("IncrementMessages called"))
                return;

            if (e.Data.Contains(" NzbDrone."))
            {
                Console.WriteLine(e.Data);
                return;
            }

            IISLogger.Trace(e.Data);
        }

        private void UpdateIISConfig()
        {
            string configPath = Path.Combine(_configProvider.IISFolder, @"AppServer\applicationhost.config");

            Logger.Info(@"Server configuration file: {0}", configPath);
            Logger.Info(@"Configuring server to: [http://localhost:{0}]", _configProvider.Port);

            var configXml = XDocument.Load(configPath);

            var bindings =
                configXml.XPathSelectElement("configuration/system.applicationHost/sites").Elements("site").Where(
                    d => d.Attribute("name").Value.ToLowerInvariant() == "nzbdrone").First().Element("bindings");
            bindings.Descendants().Remove();
            bindings.Add(
                new XElement("binding",
                             new XAttribute("protocol", "http"),
                             new XAttribute("bindingInformation", String.Format("*:{0}:localhost", _configProvider.Port))
                    ));

            bindings.Add(
            new XElement("binding",
                         new XAttribute("protocol", "http"),
                         new XAttribute("bindingInformation", String.Format("*:{0}:", _configProvider.Port))
                ));

            configXml.Save(configPath);
        }

        private void KillProcess(Process process)
        {
            if (process != null && !process.HasExited)
            {
                Logger.Info("[{0}]Killing process", process.Id);
                process.Kill();
                Logger.Info("[{0}]Waiting for exit", process.Id);
                process.WaitForExit();
                Logger.Info("[{0}]Process terminated successfully", process.Id);
            }
        }

        public string NormalizePath(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path can not be null or empty");

            var info = new FileInfo(path);

            if (info.FullName.StartsWith(@"\\")) //UNC
            {
                return info.FullName.TrimEnd('/', '\\', ' ');
            }

            return info.FullName.Trim('/', '\\', ' ').ToLower();
        }


    }
}