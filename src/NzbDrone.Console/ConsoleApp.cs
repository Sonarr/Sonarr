using System;
using System.Net.Sockets;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Host;
using NzbDrone.Host.AccessControl;

namespace NzbDrone.Console
{
    public static class ConsoleApp
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(ConsoleApp));

        private enum ExitCodes : int
        {
            Normal = 0,
            UnknownFailure = 1,
            RecoverableFailure = 2,
            NonRecoverableFailure = 3
        }

        public static void Main(string[] args)
        {
            try
            {
                var startupArgs = new StartupContext(args);
                try
                {
                    NzbDroneLogger.Register(startupArgs, false, true);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("NLog Exception: " + ex.ToString());
                    throw;
                }
                Bootstrap.Start(startupArgs, new ConsoleAlerts());
            }
            catch (SonarrStartupException ex)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(ex, "EPIC FAIL!");
                Exit(ExitCodes.NonRecoverableFailure);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(ex.Message + ". This can happen if another instance of Sonarr is already running another application is using the same port (default: 8989) or the user has insufficient permissions");
                Exit(ExitCodes.RecoverableFailure);
            }
            catch (RemoteAccessException ex)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(ex, "EPIC FAIL!");
                Exit(ExitCodes.Normal);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(ex, "EPIC FAIL!");
                Exit(ExitCodes.UnknownFailure);
            }
            
            Logger.Info("Exiting main.");

            Exit(ExitCodes.Normal);
        }

        private static void Exit(ExitCodes exitCode)
        {
            LogManager.Shutdown();

            if (exitCode != ExitCodes.Normal)
            {
                System.Console.WriteLine("Press enter to exit...");

                System.Threading.Thread.Sleep(1000);

                if (exitCode == ExitCodes.NonRecoverableFailure)
                {
                    System.Console.WriteLine("Non-recoverable failure, waiting for user intervention...");
                    for (int i = 0; i < 3600; i++)
                    {
                        System.Threading.Thread.Sleep(1000);

                        if (System.Console.KeyAvailable) break;
                    }
                }

                // Please note that ReadLine silently succeeds if there is no console, KeyAvailable does not.
                System.Console.ReadLine();
            }

            Environment.Exit((int)exitCode);
        }
    }
}
