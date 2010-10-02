//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.htm file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Collections;
using System.Configuration.Install;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CassiniDev.UIComponents;

#endregion

namespace CassiniDev
{
    public sealed class Program
    {


        [STAThread, LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        public static int Main(string[] cmdLine)
        {
            Server server = null;

            if (cmdLine != null && cmdLine.Length > 0)
            {
                bool isVS = Assembly.GetExecutingAssembly()
                    .GetName().Name.StartsWith("WEBDEV.WEBSERVER", StringComparison.OrdinalIgnoreCase);

                CommandLineArguments args = new CommandLineArguments();

                if (!CommandLineParser.ParseArgumentsWithUsage(cmdLine, args))
                {
                    if (isVS)
                    {
                        // will display vs usage and return a code that VS understands
                        return ValidateForVS(cmdLine);
                    }

                    string usage = CommandLineParser.ArgumentsUsage(typeof(CommandLineArguments), 120);
                    ShowMessage(usage, MessageBoxIcon.Asterisk);
                    return 0;
                }


                if (args.RunMode == RunMode.Hostsfile)
                {
                    SetHostsFile(args);
                    return 0;
                }


                // now we validate for us.
                int returnValue = -1;
                string message = null;

                try
                {
                    args.VisualStudio = isVS;
                    args.Validate();
                }
                catch (CassiniException ex)
                {
                    switch (ex.Message)
                    {
                        case SR.ErrNoAvailablePortFound:
                        case SR.ErrApplicationPathIsNull:
                            message = ex.Message;
                            break;
                        case SR.ErrInvalidIPMode:
                            message = SR.GetString(ex.Message, args.IPMode);
                            break;
                        case SR.ErrInvalidPortMode:
                            message = SR.GetString(ex.Message, args.PortMode);
                            break;
                        case SR.ErrPortIsInUse:
                            message = SR.GetString(ex.Message, args.Port);
                            break;
                        case SR.ErrPortRangeEndMustBeEqualOrGreaterThanPortRangeSta:
                            message = SR.GetString(ex.Message, args.PortRangeStart, args.PortRangeEnd);
                            break;
                        case SR.ErrInvalidPortRangeValue:
                            message = SR.GetString(ex.Message,
                                                   ex.Field == ErrorField.PortRangeStart
                                                       ? "start " + args.PortRangeStart
                                                       : "end " + args.PortRangeEnd);
                            break;
                        case SR.ErrInvalidHostname:
                            message = SR.GetString(ex.Message, args.HostName);
                            break;
                        case SR.WebdevDirNotExist:
                            message = SR.GetString(ex.Message, args.ApplicationPath);
                            returnValue = -2;
                            break;
                        case SR.ErrPortOutOfRange:
                            message = SR.GetString(ex.Message, args.Port);
                            break;
                    }

                    if (!args.Silent)
                    {
                        ShowMessage(message, MessageBoxIcon.Asterisk);
                    }
                    return returnValue;
                }
                catch (Exception exception)
                {
                    if (!args.Silent)
                    {
                        ShowMessage(SR.GetString(SR.ErrFailedToStartCassiniDevServerOnPortError, args.Port,
                                                 exception.Message, exception.HelpLink), MessageBoxIcon.Error);
                    }
                    return -1;
                }



                server = new Server(args.Port, args.VirtualPath, args.ApplicationPath, args.Ntlm, args.Nodirlist);


                if (args.AddHost)
                {
                    HostsFile.AddHostEntry(server.IPAddress.ToString(), server.HostName);
                }

                try
                {
                    server.Start();
                }
                catch (Exception exception)
                {
                    if (!args.Silent)
                    {
                        ShowMessage(SR.GetString(SR.ErrFailedToStartCassiniDevServerOnPortError, args.Port,
                                                 exception.Message, exception.HelpLink), MessageBoxIcon.Error);
                    }
                    return -4;
                }

            }

            using (FormView form = new FormView(server))
            {
                Application.Run(form);
            }

            return 0;
        }

        private static void ShowMessage(string msg, MessageBoxIcon icon)
        {
            if (Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                MessageBox.Show(msg, SR.GetString(SR.WebdevName), MessageBoxButtons.OK, icon,
                                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
            }
            else
            {
                MessageBox.Show(msg, SR.GetString(SR.WebdevName), MessageBoxButtons.OK, icon);
            }
        }

        private static void ShowMessage(string msg)
        {
            if (Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                MessageBox.Show(msg, SR.GetString(SR.WebdevName), MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
            }
            else
            {
                MessageBox.Show(msg, SR.GetString(SR.WebdevName), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        /// <summary>
        /// TODO: update usage will all arguments and present on a legible substrate.
        /// </summary>
        private static void ShowUsage()
        {
            ShowMessage(SR.GetString(SR.WebdevUsagestr1) + SR.GetString(SR.WebdevUsagestr2) +
                        SR.GetString(SR.WebdevUsagestr3) +
                        SR.GetString(SR.WebdevUsagestr4) + SR.GetString(SR.WebdevUsagestr5) +
                        SR.GetString(SR.WebdevUsagestr6) +
                        SR.GetString(SR.WebdevUsagestr7), MessageBoxIcon.Asterisk);
        }


        /// <summary>
        /// Keeping the VS validation to return codes that it likes.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static int ValidateForVS(string[] args)
        {
            CommandLine line = new CommandLine(args);

            bool flag = line.Options["silent"] != null;

            if (!flag && line.ShowHelp)
            {
                ShowUsage();
                return 0;
            }

            string virtualPath = (string)line.Options["vpath"];

            if (virtualPath != null)
            {
                virtualPath = virtualPath.Trim();
            }

            // we are being a bit generous here as CommandLineArguments can handle these sitchiashuns
            if (string.IsNullOrEmpty(virtualPath))
            {
                virtualPath = "/";
            }

            else if (!virtualPath.StartsWith("/", StringComparison.Ordinal))
            {
                if (!flag)
                {
                    ShowUsage();
                }
                return -1;
            }

            string path = (string)line.Options["path"];

            if (path != null)
            {
                path = path.Trim();
            }

            if (string.IsNullOrEmpty(path))
            {
                if (!flag)
                {
                    ShowUsage();
                }
                return -1;
            }

            path = Path.GetFullPath(path);

            if (!Directory.Exists(path))
            {
                if (!flag)
                {
                    ShowMessage(SR.GetString(SR.WebdevDirNotExist, new object[] { path }));
                }
                return -2;
            }

            int port = 0;

            string s = (string)line.Options["port"];

            if (s != null)
            {
                s = s.Trim();
            }

            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    port = int.Parse(s, CultureInfo.InvariantCulture);
                    if ((port < 1) || (port > 0xffff))
                    {
                        if (!flag)
                        {
                            ShowUsage();
                        }
                        return -1;
                    }

                }
                catch
                {
                    if (!flag)
                    {
                        ShowMessage(SR.GetString(SR.WebdevInvalidPort, new object[] { s }));
                    }
                    return -3;
                }
            }

            if (!flag)
            {
                ShowUsage();
            }

            return 0;
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

    internal sealed class ServiceUtil
    {
        /// <summary>
        /// http://stackoverflow.com/questions/1449994/inno-setup-for-windows-service
        /// http://groups.google.co.uk/group/microsoft.public.dotnet.languages.csharp/browse_thread/thread/4d45e9ea5471cba4/4519371a77ed4a74
        /// </summary>
        /// <param name="undo"></param>
        /// <param name="args"></param>
        public static void Install(bool undo, string[] args)
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