using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NzbDrone
{
    static class Program
    {

        private static readonly Logger Logger = LogManager.GetLogger("Application");
        private static readonly Logger IISLogger = LogManager.GetLogger("IISExpress");


        private static Process IISProcess;

        static void Main()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e));
                var projectRoot = GetProjectRoot();
                ConfigureNlog();
                Logger.Info("Starting NZBDrone. Start-up Path:'{0}'", projectRoot);

                var iisPath = Path.Combine(projectRoot, @"IISExpress\iisexpress.exe");
                Logger.Info("IISExpress Path:'{0}'", iisPath);

                KillOrphane(iisPath);

                Logger.Info("Preparing IISExpress Server...");
                IISProcess = new Process();

                IISProcess.StartInfo.FileName = iisPath;
                IISProcess.StartInfo.Arguments = "/config:IISExpress\\Appserver\\applicationhost.config";
                IISProcess.StartInfo.WorkingDirectory = projectRoot;

                IISProcess.StartInfo.UseShellExecute = false;
                IISProcess.StartInfo.RedirectStandardOutput = true;
                IISProcess.StartInfo.RedirectStandardError = true;
                IISProcess.StartInfo.CreateNoWindow = true;

                IISProcess.OutputDataReceived += ((s, e) => IISLogger.Trace(e.Data));
                IISProcess.ErrorDataReceived += ((s, e) => IISLogger.Fatal(e.Data));

                //Set Variables for the config file.
                Environment.SetEnvironmentVariable("NZBDRONE_PATH", projectRoot);

                Logger.Info("Starting process");
                IISProcess.Start();

                //Event handlers, try to terminate iis express before exiting application.
                AppDomain.CurrentDomain.ProcessExit += ProgramExited;
                AppDomain.CurrentDomain.DomainUnload += ProgramExited;
                Process.GetCurrentProcess().Exited += ProgramExited;

                IISProcess.BeginErrorReadLine();
                IISProcess.BeginOutputReadLine();

                IISProcess.WaitForExit();
                Logger.Info("Main IISExpress instance was terminated. ExitCode:{0}", IISProcess.ExitCode);
            }
            catch (Exception e)
            {
                AppDomainException(e);
            }

            Console.Write("Press Enter To Exit...");
            Console.ReadLine();

        }

        private static string GetProjectRoot()
        {
            var appDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

            while (appDir.GetDirectories("iisexpress").Length == 0)
            {
                if (appDir.Parent == null) throw new ApplicationException("Can't fine IISExpress folder.");
                appDir = appDir.Parent;
            }

            return appDir.FullName;

        }

        private static void AppDomainException(object excepion)
        {
            Console.WriteLine("EPIC FAIL: {0}", excepion);
            Logger.Fatal("EPIC FAIL: {0}", excepion);
            KillProcess(IISProcess);
        }

        static void ProgramExited(object sender, EventArgs e)
        {
            KillProcess(IISProcess);
        }

        private static void KillOrphane(string path)
        {
            Logger.Trace("================================================");
            Logger.Info("Finding orphaned IIS Processes.");
            foreach (var process in Process.GetProcessesByName("IISExpress"))
            {
                Logger.Trace("-------------------------");
                string processPath = process.MainModule.FileName;
                Logger.Info("[{0}]IIS Process found. Path:{1}", process.Id, processPath);
                if (CleanPath(processPath) == CleanPath(path))
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
            if (process != null)
            {
                Logger.Info("[{0}]Killing process", process.Id);
                process.Kill();
                Logger.Info("[{0}]Waiting for exit", process.Id);
                process.WaitForExit();
                Logger.Info("[{0}]Process terminated successfully", process.Id);
            }
        }

        private static string CleanPath(string path)
        {
            return path.ToLower().Replace("\\", "").Replace("//", "//");
        }

        private static void ConfigureNlog()
        {
            var config = new LoggingConfiguration();

            var debuggerTarget = new DebuggerTarget
             {
                 Layout = "${logger}: ${message}"
             };


            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = "${logger}: ${message}"
            };


            config.AddTarget("debugger", debuggerTarget);
            config.AddTarget("console", consoleTarget);
            //config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            // Step 4. Define rules
            //LoggingRule fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            var debugRule = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
            var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);

            //config.LoggingRules.Add(fileRule);
            config.LoggingRules.Add(debugRule);
            config.LoggingRules.Add(consoleRule);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }
    }
}

