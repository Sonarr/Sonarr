using System;
using System.IO;
using System.Net.Sockets;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Host;
using NzbDrone.Host.AccessControl;
using NzbDrone.RuntimePatches;

namespace NzbDrone.Console
{
    public static class ConsoleApp
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(ConsoleApp));

        private enum ExitCodes
        {
            Normal = 0,
            UnknownFailure = 1,
            RecoverableFailure = 2,
            NonRecoverableFailure = 3
        }

        public static void Main(string[] args)
        {
            RuntimePatcher.Initialize();
            StartupContext startupArgs = null;

            try
            {
                startupArgs = new StartupContext(args);
                try
                {
                    NzbDroneLogger.Register(startupArgs, false, true);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("NLog Exception: " + ex.ToString());
                    throw;
                }

                Bootstrap.Start(args);
            }
            catch (SonarrStartupException ex)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(ex, "EPIC FAIL!");
                Exit(ExitCodes.NonRecoverableFailure, startupArgs);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(ex.Message + ". This can happen if another instance of Sonarr is already running another application is using the same port (default: 8989) or the user has insufficient permissions");
                Exit(ExitCodes.RecoverableFailure, startupArgs);
            }
            catch (IOException ex)
            {
                if (ex.InnerException is AddressInUseException)
                {
                    System.Console.WriteLine("");
                    System.Console.WriteLine("");
                    Logger.Fatal(ex.Message + " This can happen if another instance of Sonarr is already running another application is using the same port (default: 8989) or the user has insufficient permissions");
                    Exit(ExitCodes.RecoverableFailure, startupArgs);
                }
                else
                {
                    throw;
                }
            }
            catch (RemoteAccessException ex)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(ex, "EPIC FAIL!");
                Exit(ExitCodes.Normal, startupArgs);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(ex, "EPIC FAIL!");
                System.Console.WriteLine("EPIC FAIL! " + ex.ToString());
                Exit(ExitCodes.UnknownFailure, startupArgs);
            }

            Logger.Info("Exiting main.");

            Exit(ExitCodes.Normal, startupArgs);
        }

        private static void Exit(ExitCodes exitCode, StartupContext startupArgs)
        {
            LogManager.Shutdown();

            if (exitCode != ExitCodes.Normal)
            {
                System.Console.WriteLine("Press enter to exit...");

                System.Threading.Thread.Sleep(1000);

                if (exitCode == ExitCodes.NonRecoverableFailure)
                {
                    if (startupArgs?.ExitImmediately == true)
                    {
                        System.Console.WriteLine("Non-recoverable failure, but set to exit immediately");

                        Environment.Exit((int)exitCode);
                    }

                    System.Console.WriteLine("Non-recoverable failure, waiting for user intervention...");
                    for (int i = 0; i < 3600; i++)
                    {
                        System.Threading.Thread.Sleep(1000);

                        if (System.Console.KeyAvailable)
                        {
                            break;
                        }
                    }
                }

                // Please note that ReadLine silently succeeds if there is no console, KeyAvailable does not.
                System.Console.ReadLine();
            }

            Environment.Exit((int)exitCode);
        }
    }
}
