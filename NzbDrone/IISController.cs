using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NLog;

namespace NzbDrone
{
    class IISController
    {
        public static Process IISProcess { get; private set; }
        private static readonly Logger IISLogger = LogManager.GetLogger("IISExpress");
        private static readonly Logger Logger = LogManager.GetLogger("IISController");
        private static readonly string IISFolder = Path.Combine(Config.ProjectRoot, @"IISExpress\");
        private static readonly string IISExe = Path.Combine(IISFolder, @"iisexpress.exe");


        internal static string AppUrl
        {
            get { return string.Format("http://localhost:{0}/", Config.Port); }
        }

        internal static Process StartIIS()
        {
            Logger.Info("Preparing IISExpress Server...");
            IISProcess = new Process();

            IISProcess.StartInfo.FileName = IISExe;
            IISProcess.StartInfo.Arguments = "/config:IISExpress\\Appserver\\applicationhost.config /trace:i";
            IISProcess.StartInfo.WorkingDirectory = Config.ProjectRoot;

            IISProcess.StartInfo.UseShellExecute = false;
            IISProcess.StartInfo.RedirectStandardOutput = true;
            IISProcess.StartInfo.RedirectStandardError = true;
            IISProcess.StartInfo.CreateNoWindow = true;


            IISProcess.OutputDataReceived += (OnDataReceived);

            IISProcess.ErrorDataReceived += ((s, e) => IISLogger.Fatal(e.Data));



            //Set Variables for the config file.
            Environment.SetEnvironmentVariable("NZBDRONE_PATH", Config.ProjectRoot);

            try
            {
                UpdateIISConfig();
            }
            catch (Exception e)
            {
                Logger.Error("An error has occured while trying to update the config file.", e);
            }


            Logger.Info("Starting process. [{0}]", IISProcess.StartInfo.FileName);
            IISProcess.Start();

            IISProcess.BeginErrorReadLine();
            IISProcess.BeginOutputReadLine();
            return IISProcess;
        }

        private static void OnDataReceived(object s, DataReceivedEventArgs e)
        {
            if (e == null || e.Data == null || e.Data.StartsWith("Request started:") || e.Data.StartsWith("Request ended:") || e.Data == ("IncrementMessages called"))
                return;

            IISLogger.Trace(e.Data);
        }

        internal static void StopIIS()
        {
            KillProcess(IISProcess);
        }

        internal static void KillOrphaned()
        {
            Logger.Info("Finding orphaned IIS Processes.");
            foreach (var process in Process.GetProcessesByName("IISExpress"))
            {
                string processPath = process.MainModule.FileName;
                Logger.Info("[{0}]IIS Process found. Path:{1}", process.Id, processPath);
                if (CleanPath(processPath) == CleanPath(IISExe))
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

        private static void KillProcess(Process process)
        {
            if (process == null) return;

            Logger.Info("[{0}]Killing process", process.Id);
            process.Kill();
            Logger.Info("[{0}]Waiting for exit", process.Id);
            process.WaitForExit();
            Logger.Info("[{0}]Process terminated successfully", process.Id);
        }

        private static void UpdateIISConfig()
        {
            string configPath = Path.Combine(IISFolder, @"AppServer\applicationhost.config");

            Logger.Info(@"Server configuration file: {0}", configPath);
            Logger.Info(@"Configuring server to: [http://localhost:{0}]", Config.Port);

            var configXml = XDocument.Load(configPath);

            var bindings = configXml.XPathSelectElement("configuration/system.applicationHost/sites").Elements("site").Where(d => d.Attribute("name").Value.ToLowerInvariant() == "nzbdrone").First().Element("bindings");
            bindings.Descendants().Remove();
            bindings.Add(
            new XElement("binding",
            new XAttribute("protocol", "http"),
            new XAttribute("bindingInformation", String.Format("*:{0}:", Config.Port))
            ));

            configXml.Save(configPath);
        }

        private static string CleanPath(string path)
        {
            return path.ToLower().Replace("\\", "").Replace("//", "//");
        }
    }
}
