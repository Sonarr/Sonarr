using System;
using System.Diagnostics;
using System.Threading;
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
                Logger.Info("Starting NZBDrone. Start-up Path:'{0}'", Config.ProjectRoot);
                Thread.CurrentThread.Name = "Host";

                Process currentProcess = Process.GetCurrentProcess();

                FixPriorities();

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
                FixPriorities();

                if (Environment.UserInteractive)
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

        private static void FixPriorities()
        {
            Process currentProcess = Process.GetCurrentProcess();
            if (currentProcess.PriorityClass < ProcessPriorityClass.Normal)
            {
                Logger.Info("Promoting process priority from {0} to {1}", currentProcess.PriorityClass,
                            ProcessPriorityClass.Normal);
                currentProcess.PriorityClass = ProcessPriorityClass.Normal;
            }

            
            if (IISController.IISProcess!=null && IISController.IISProcess.PriorityClass < ProcessPriorityClass.Normal)
            {
                Logger.Info("Promoting process priority from {0} to {1}", IISController.IISProcess.PriorityClass,
                            ProcessPriorityClass.Normal);
                IISController.IISProcess.PriorityClass = ProcessPriorityClass.Normal;
            }
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