using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;
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
            IISProcess.StartInfo.Arguments = "/config:IISExpress\\Appserver\\applicationhost.config";
            IISProcess.StartInfo.WorkingDirectory = Config.ProjectRoot;

            IISProcess.StartInfo.UseShellExecute = false;
            IISProcess.StartInfo.RedirectStandardOutput = true;
            IISProcess.StartInfo.RedirectStandardError = true;
            IISProcess.StartInfo.CreateNoWindow = true;

            IISProcess.OutputDataReceived += ((s, e) => IISLogger.Trace(e.Data));
            IISProcess.ErrorDataReceived += ((s, e) => IISLogger.Fatal(e.Data));

            //Set Variables for the config file.
            Environment.SetEnvironmentVariable("NZBDRONE_PATH", Config.ProjectRoot);
            UpdateIISConfig();

            Logger.Info("Starting process. [{0}]", IISProcess.StartInfo.FileName);
            IISProcess.Start();

            IISProcess.BeginErrorReadLine();
            IISProcess.BeginOutputReadLine();
            return IISProcess;
        }

        internal static void StopIIS()
        {
            KillProcess(IISProcess);
        }

        internal static void KillOrphaned()
        {
            Logger.Trace("================================================");
            Logger.Info("Finding orphaned IIS Processes.");
            foreach (var process in Process.GetProcessesByName("IISExpress"))
            {
                Logger.Trace("-------------------------");
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
                Logger.Trace("-------------------------");
            }
            Logger.Trace("================================================");
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
            Logger.Info(@"Configuring server to: [http://localhost:{0}]", Config.Port);
            var serverManager = new ServerManager(Path.Combine(IISFolder, @"AppServer\applicationhost.config"));
            serverManager.Sites["NZBDrone"].Bindings[0].BindingInformation = string.Format("*:{0}:", Config.Port);
            serverManager.CommitChanges();
        }

        private static string CleanPath(string path)
        {
            return path.ToLower().Replace("\\", "").Replace("//", "//");
        }
    }
}
