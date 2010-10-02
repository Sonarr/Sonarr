// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.Collections;
using System.Configuration.Install;
using System.Net;




namespace CassiniDev
{
    public class Program
    {
        [STAThread]
        private static void Main(string[] cmdLine)
        {
            CommandLineArguments args = new CommandLineArguments();
 

            if (!CommandLineParser.ParseArgumentsWithUsage(cmdLine, args))
            {
                Environment.Exit(-1);
            }
            else
            {
                switch (args.RunMode)
                {
                    case RunMode.Server:
                        IPAddress ip=IPAddress.Loopback;
                        try
                        {
                            args.Validate();

                            ip = CommandLineArguments.ParseIP(args.IPMode, args.IPv6, args.IPAddress);
                            int port = args.PortMode == PortMode.FirstAvailable ?
                                CassiniNetworkUtils.GetAvailablePort(args.PortRangeStart, args.PortRangeEnd, ip, true) : 
                                args.Port;

                            if(args.AddHost)
                            {
                                HostsFile.AddHostEntry(ip.ToString(), args.HostName);
                            }

                            using (var server =
                                new Server(port, args.VirtualPath, args.ApplicationPath,
                                    ip, args.HostName, args.TimeOut))
                            {
                                server.Start();
                                Console.WriteLine("started: {0}\r\nPress Enter key to exit....", server.RootUrl);
                                Console.ReadLine();
                                server.ShutDown();
                            }
                        }
                        catch (CassiniException ex)
                        {
                            Console.WriteLine("error:{0} {1}",
                                              ex.Field == ErrorField.None
                                                  ? ex.GetType().Name
                                                  : ex.Field.ToString(), ex.Message);
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine("error:{0}", ex2.Message);
                            Console.WriteLine(CommandLineParser.ArgumentsUsage(typeof(CommandLineArguments)));
                        }
                        finally
                        {
                            if (args.AddHost)
                            {
                                HostsFile.RemoveHostEntry(ip.ToString(), args.HostName);
                            }

                        }
                        break;
                    case RunMode.Hostsfile:
                        SetHostsFile(args);
                        break;
                }
            }
        }

        private static void SetHostsFile(CommandLineArguments sargs)
        {
            try
            {
                if (sargs.AddHost)
                {
                    HostsFile.AddHostEntry(sargs.IPAddress, sargs.HostName);
                }
                else
                {
                    HostsFile.RemoveHostEntry(sargs.IPAddress, sargs.HostName);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Environment.Exit(-1);
            }
            catch
            {
                Environment.Exit(-2);
            }
        }
    }

    public sealed class ServiceUtil
    {
        static void Install(bool undo, string[] args)
        {
            try
            {
                Console.WriteLine(undo ? "uninstalling" : "installing");
                using (AssemblyInstaller inst = new AssemblyInstaller(typeof(Program).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (undo)
                        {
                            inst.Uninstall(state);
                        }
                        else
                        {
                            inst.Install(state);
                            inst.Commit(state);
                        }
                    }
                    catch
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch { }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}