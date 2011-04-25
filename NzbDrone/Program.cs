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
            Logger.Info(Process.GetCurrentProcess().Id);

            try
            {
                Thread.CurrentThread.Name = "Host";

                AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e));
                AppDomain.CurrentDomain.ProcessExit += ProgramExited;
                AppDomain.CurrentDomain.DomainUnload += ProgramExited;
                Process.GetCurrentProcess().EnableRaisingEvents = true;
                Process.GetCurrentProcess().Exited += ProgramExited;

                Config.ConfigureNlog();

                Logger.Info("Starting NZBDrone. Start-up Path:'{0}'", Config.ProjectRoot);

                IISController.StopServer();
                IISController.StartServer();



#if DEBUG
                Attach();
#endif

                Process.Start(IISController.AppUrl);

                while (true)
                {
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                AppDomainException(e);
            }
        }

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


        private static void AppDomainException(object excepion)
        {
            Console.WriteLine("EPIC FAIL: {0}", excepion);
            Logger.Fatal("EPIC FAIL: {0}", excepion);

            new Client
                {
                    ApiKey = "43BBF60A-EB2A-4C1C-B09E-422ADF637265",
                    ApplicationName = "NZBDrone",
                    CurrentException = excepion as Exception
                }.Submit();

            IISController.StopServer();
        }

        private static void ProgramExited(object sender, EventArgs e)
        {
            IISController.StopServer();
        }
    }
}