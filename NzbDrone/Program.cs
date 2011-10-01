using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Timers;
using Exceptioneer.WindowsFormsClient;
using NLog;
namespace NzbDrone
{
    internal static class Program
    {
        private static readonly Logger Logger = LogManager.GetLogger("Application");

        private static void Main()
        {
            try
            {
                Config.ConfigureNlog();
                Config.CreateDefaultConfigFile();
                Logger.Info("Starting NZBDrone. Start-up Path:'{0}'", Config.ProjectRoot);
                Thread.CurrentThread.Name = "Host";

                Process currentProcess = Process.GetCurrentProcess();

                var prioCheckTimer = new System.Timers.Timer(5000);
                prioCheckTimer.Elapsed += prioCheckTimer_Elapsed;
                prioCheckTimer.Enabled = true;

                currentProcess.EnableRaisingEvents = true;
                currentProcess.Exited += ProgramExited;

                AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e));
                AppDomain.CurrentDomain.ProcessExit += ProgramExited;
                AppDomain.CurrentDomain.DomainUnload += ProgramExited;

                IISController.StopServer();
                IISController.StartServer();

#if DEBUG
                Attach();
#endif

                if (!Environment.UserInteractive || !Config.LaunchBrowser)
                {
                    try
                    {
                        new WebClient().DownloadString(IISController.AppUrl);
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorException("Failed to load home page.", e);
                    }
                }
                else
                {
                    try
                    {
                        Logger.Info("Starting default browser. {0}", IISController.AppUrl);
                        Process.Start(IISController.AppUrl);
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorException("Failed to open URL in default browser.", e);
                    }
                    while (true)
                    {
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                AppDomainException(e);
            }

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void prioCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Process currentProcess = Process.GetCurrentProcess();
            if (currentProcess.PriorityClass != ProcessPriorityClass.Normal)
            {
                SetPriority(currentProcess);
            }


            if (IISController.IISProcess != null)
            {
                IISController.IISProcess.Refresh();

                if (IISController.IISProcess.PriorityClass != ProcessPriorityClass.Normal && IISController.IISProcess.PriorityClass != ProcessPriorityClass.AboveNormal)
                {
                    SetPriority(IISController.IISProcess);
                }
            }
        }

        private static void SetPriority(Process process)
        {
            Logger.Info("Updating [{0}] process priority from {1} to {2}",
                          process.ProcessName,
                          IISController.IISProcess.PriorityClass,
                          ProcessPriorityClass.Normal);
            process.PriorityClass = ProcessPriorityClass.Normal;
        }



#if DEBUG
        private static void Attach()
        {
            if (Debugger.IsAttached)
            {
                Logger.Info("Trying to attach to debugger");

                var count = 0;

                while (true)
                {
                    try
                    {
                        ProcessAttacher.Attach();
                        Logger.Info("Debugger Attached");
                        return;
                    }
                    catch (Exception e)
                    {
                        count++;
                        if (count > 20)
                        {
                            Logger.WarnException("Unable to attach to debugger", e);
                            return;
                        }

                        Thread.Sleep(100);

                    }
                }
            }
        }
#endif

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

            IISController.StopServer();
        }

        private static void ProgramExited(object sender, EventArgs e)
        {
            IISController.StopServer();
        }
    }
}