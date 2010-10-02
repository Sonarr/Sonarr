//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

#endregion

namespace CassiniDev
{
    /// <summary>
    /// Command line arguments
    /// 
    /// fixed 5/24/10 - quoted embedded spaces in ToString
    /// </summary>
    public class CommandLineArguments
    {
        #region Properties

        [Argument(ArgumentType.AtMostOnce, ShortName = "ah", DefaultValue = false,
            HelpText = "If true add entry to Windows hosts file. Requires write permissions to hosts file.")] public
            bool AddHost;

        [Argument(ArgumentType.AtMostOnce, ShortName = "a", LongName = "path",
            HelpText = "Physical location of content.")] public string
            ApplicationPath;

        [Argument(ArgumentType.AtMostOnce, LongName = "log", DefaultValue = false, HelpText = "Enable logging.")] public
            bool EnableLogging;

        [Argument(ArgumentType.AtMostOnce, ShortName = "h", LongName = "host",
            HelpText = "Host name used for app root url. Optional unless AddHost is true.")] public string HostName;

        [Argument(ArgumentType.AtMostOnce, ShortName = "i", LongName = "ip",
            HelpText = "IP address to listen to. Ignored if IPMode != Specific")] public string IPAddress;

        [Argument(ArgumentType.AtMostOnce, ShortName = "im", DefaultValue = IPMode.Loopback, HelpText = "",
            LongName = "ipMode")] public
            IPMode IPMode;

        [Argument(ArgumentType.AtMostOnce, ShortName = "v6", DefaultValue = false,
            HelpText = "If IPMode 'Any' or 'LoopBack' are specified use the V6 address", LongName = "ipV6")] public bool
            IPv6;


        [Argument(ArgumentType.AtMostOnce, LongName = "nodirlist", DefaultValue = false,
            HelpText = "Disable diretory listing")] public bool Nodirlist;

        [Argument(ArgumentType.AtMostOnce, LongName = "ntlm", DefaultValue = false, HelpText = "Run as current identity"
            )] public bool Ntlm;


        [Argument(ArgumentType.AtMostOnce, ShortName = "p", LongName = "port",
            HelpText = "Port to listen to. Ignored if PortMode=FirstAvailable.", DefaultValue = 0)] public int Port;

        [Argument(ArgumentType.AtMostOnce, ShortName = "pm", HelpText = "", LongName = "portMode",
            DefaultValue = PortMode.FirstAvailable)] public PortMode PortMode;

        [Argument(ArgumentType.AtMostOnce, ShortName = "pre", DefaultValue = 65535, LongName = "highPort",
            HelpText = "End of port range. Ignored if PortMode != FirstAvailable")] public int PortRangeEnd = 9000;

        [Argument(ArgumentType.AtMostOnce, ShortName = "prs", DefaultValue = 32768, LongName = "lowPort",
            HelpText = "Start of port range. Ignored if PortMode != FirstAvailable")] public int PortRangeStart =
                8080;

        [DefaultArgument(ArgumentType.AtMostOnce, DefaultValue = RunMode.Server, HelpText = "[Server|Hostsfile]")] public RunMode RunMode;
        [Argument(ArgumentType.AtMostOnce, LongName = "silent", DefaultValue = false, HelpText = "Fail silently")] public bool Silent;

        [Argument(ArgumentType.AtMostOnce, ShortName = "t", DefaultValue = 0, LongName = "timeout",
            HelpText = "Length of time, in ms, to wait for a request before stopping the server. 0 = no timeout.")] public int TimeOut;

        [Argument(ArgumentType.AtMostOnce, ShortName = "v", LongName = "vpath", DefaultValue = "/",
            HelpText = "Optional. default value '/'"
            )] public string VirtualPath = "/";

        [Argument(ArgumentType.AtMostOnce, ShortName = "vs", DefaultValue = false,
            HelpText = "If true run in Visual Studio Development Server mode - readonly UI with single option to quit.."
            )] public
            bool VisualStudio;

        [Argument(ArgumentType.AtMostOnce, ShortName = "w", DefaultValue = 0, LongName = "wait",
            HelpText =
                "Length of time, in ms, to wait for a specific port before throwing an exception or exiting. 0 = don't wait."
            )] public int WaitForPort;

        #endregion

        public string[] ToArgs()
        {
            List<string>  result = new List<string>();
            if (RunMode != RunMode.Server)
            {
                result.Add(string.Format("{0}", RunMode));
            }
            if (!string.IsNullOrEmpty(ApplicationPath))
            {
                result.Add(string.Format("/a:{0}", ApplicationPath.Contains("") ? String.Format("\"{0}\"", ApplicationPath) : ApplicationPath));
            }
            result.Add(string.Format("/v:{0}", VirtualPath.Contains("") ? String.Format("\"{0}\"", VirtualPath) : VirtualPath));

            if (!string.IsNullOrEmpty(HostName))
            {
                result.Add(string.Format("/h:{0}", HostName.Contains("") ? String.Format("\"{0}\"", HostName) : HostName));
            }
            if (AddHost)
            {
                result.Add("/ah");
            }

            if (IPMode != IPMode.Loopback)
            {
                result.Add(string.Format("/im:{0}", IPMode));
            }

            if (!string.IsNullOrEmpty(IPAddress))
            {
                result.Add(string.Format("/i:{0}", IPAddress));
            }

            if (IPv6)
            {
                result.Add("/v6");
            }

            if (VisualStudio)
            {
                result.Add("/vs");
            }

            if (PortMode != PortMode.FirstAvailable)
            {
                result.Add(string.Format("/pm:{0}", PortMode));
            }

            if (Port != 0)
            {
                result.Add(string.Format("/p:{0}", Port));
            }

            if (PortRangeStart != 32768)
            {
                result.Add(string.Format("/prs:{0}", PortRangeStart));
            }
            if (PortRangeEnd != 65535)
            {
                result.Add(string.Format("/pre:{0}", PortRangeEnd));
            }
            if (TimeOut > 0)
            {
                result.Add(string.Format("/t:{0}", TimeOut));
            }
            if (WaitForPort > 0)
            {
                result.Add(string.Format("/w:{0}", WaitForPort));
            }

            if (Ntlm)
            {
                result.Add("/ntlm");
            }
            if (Silent)
            {
                result.Add("/silent");
            }
            if (Nodirlist)
            {
                result.Add("/nodirlist");
            }
            if (EnableLogging)
            {
                result.Add("/log");
            }

            return result.ToArray();
        }
        public override string ToString()
        {
            return string.Join(" ", ToArgs());
            //StringBuilder sb = new StringBuilder();
            //if (RunMode != RunMode.Server)
            //{
            //    sb.AppendFormat("{0}", RunMode);
            //}
            //if (!string.IsNullOrEmpty(ApplicationPath))
            //{
            //    sb.AppendFormat(" /a:{0}", ApplicationPath.Contains(" ") ? String.Format("\"{0}\"", ApplicationPath) : ApplicationPath);
            //}
            //sb.AppendFormat(" /v:{0}", VirtualPath.Contains(" ") ? String.Format("\"{0}\"", VirtualPath) : VirtualPath);

            //if (!string.IsNullOrEmpty(HostName))
            //{
            //    sb.AppendFormat(" /h:{0}", HostName.Contains(" ") ? String.Format("\"{0}\"", HostName) : HostName);
            //}
            //if (AddHost)
            //{
            //    sb.Append(" /ah");
            //}

            //if (IPMode != IPMode.Loopback)
            //{
            //    sb.AppendFormat(" /im:{0}", IPMode);
            //}

            //if (!string.IsNullOrEmpty(IPAddress))
            //{
            //    sb.AppendFormat(" /i:{0}", IPAddress);
            //}

            //if (IPv6)
            //{
            //    sb.Append(" /v6");
            //}

            //if (VisualStudio)
            //{
            //    sb.Append(" /vs");
            //}

            //if (PortMode != PortMode.FirstAvailable)
            //{
            //    sb.AppendFormat(" /pm:{0}", PortMode);
            //}

            //if (Port != 0)
            //{
            //    sb.AppendFormat(" /p:{0}", Port);
            //}

            //if (PortRangeStart != 32768)
            //{
            //    sb.AppendFormat(" /prs:{0}", PortRangeStart);
            //}
            //if (PortRangeEnd != 65535)
            //{
            //    sb.AppendFormat(" /pre:{0}", PortRangeEnd);
            //}
            //if (TimeOut > 0)
            //{
            //    sb.AppendFormat(" /t:{0}", TimeOut);
            //}
            //if (WaitForPort > 0)
            //{
            //    sb.AppendFormat(" /w:{0}", WaitForPort);
            //}

            //if (Ntlm)
            //{
            //    sb.Append(" /ntlm");
            //}
            //if (Silent)
            //{
            //    sb.Append(" /silent");
            //}
            //if (Nodirlist)
            //{
            //    sb.Append(" /nodirlist");
            //}
            //if (EnableLogging)
            //{
            //    sb.Append(" /log");
            //}
            //return sb.ToString().Trim();
        }

        /// <summary>
        /// </summary>
        internal void Validate()
        {
            if (string.IsNullOrEmpty(ApplicationPath))
            {
                throw new CassiniException(SR.ErrApplicationPathIsNull, ErrorField.ApplicationPath);
            }

            try
            {
                ApplicationPath = Path.GetFullPath(ApplicationPath);
            }
            catch
            {
            }
            if (!Directory.Exists(ApplicationPath))
            {
                throw new CassiniException(SR.WebdevDirNotExist, ErrorField.ApplicationPath);
            }

            ApplicationPath = ApplicationPath.Trim('\"').TrimEnd('\\');


            if (!string.IsNullOrEmpty(VirtualPath))
            {
                VirtualPath = VirtualPath.Trim('\"');
                VirtualPath = VirtualPath.Trim('/');
                VirtualPath = "/" + VirtualPath;
            }
            else
            {
                VirtualPath = "/";
            }


            if (!VirtualPath.StartsWith("/"))
            {
                VirtualPath = "/" + VirtualPath;
            }


            if (AddHost && string.IsNullOrEmpty(HostName))
            {
                throw new CassiniException(SR.ErrInvalidHostname, ErrorField.HostName);
            }


            IPAddress = ParseIP(IPMode, IPv6, IPAddress).ToString();


            if (VisualStudio) // then STOP HERE.
            {
                // It is fortunate that in order to provide api parity with WebDev
                // we do not need to port scan. Visual Studio balks and refuses to 
                // attach if we monkey around and open ports.
                Port = Port == 0 ? 80 : Port;
                PortMode = PortMode.Specific;
                return;
            }


            switch (PortMode)
            {
                case PortMode.FirstAvailable:

                    if (PortRangeStart < 1)
                    {
                        throw new CassiniException(SR.ErrInvalidPortRangeValue, ErrorField.PortRangeStart);
                    }

                    if (PortRangeEnd < 1)
                    {
                        throw new CassiniException(SR.ErrInvalidPortRangeValue, ErrorField.PortRangeEnd);
                    }

                    if (PortRangeStart > PortRangeEnd)
                    {
                        throw new CassiniException(SR.ErrPortRangeEndMustBeEqualOrGreaterThanPortRangeSta,
                                                   ErrorField.PortRange);
                    }
                    Port = CassiniNetworkUtils.GetAvailablePort(PortRangeStart, PortRangeEnd,
                                                                System.Net.IPAddress.Parse(IPAddress), true);

                    if (Port == 0)
                    {
                        throw new CassiniException(SR.ErrNoAvailablePortFound, ErrorField.PortRange);
                    }

                    break;

                case PortMode.Specific:

                    if ((Port < 1) || (Port > 0xffff))
                    {
                        throw new CassiniException(SR.ErrPortOutOfRange, ErrorField.Port);
                    }


                    // start waiting....
                    //TODO: design this hack away.... why am I waiting in a validation method?
                    int now = Environment.TickCount;

                    // wait until either 1) the specified port is available or 2) the specified amount of time has passed
                    while (Environment.TickCount < now + WaitForPort &&
                           CassiniNetworkUtils.GetAvailablePort(Port, Port, System.Net.IPAddress.Parse(IPAddress), true) !=
                           Port)
                    {
                        Thread.Sleep(100);
                    }

                    // is the port available?
                    if (CassiniNetworkUtils.GetAvailablePort(Port, Port, System.Net.IPAddress.Parse(IPAddress), true) !=
                        Port)
                    {
                        throw new CassiniException(SR.ErrPortIsInUse, ErrorField.Port);
                    }


                    break;
                default:

                    throw new CassiniException(SR.ErrInvalidPortMode, ErrorField.None);
            }
        }


        /// <summary>
        /// Converts CommandLineArgument values to an IP address if possible.
        /// Throws Exception if not.
        /// </summary>
        /// <param name="ipmode"></param>
        /// <param name="v6"></param>
        /// <param name="ipString"></param>
        /// <returns></returns>
        /// <exception cref="CassiniException">If IPMode is invalid</exception>
        /// <exception cref="CassiniException">If IPMode is 'Specific' and ipString is invalid</exception>
        public static IPAddress ParseIP(IPMode ipmode, bool v6, string ipString)
        {
            IPAddress ip;
            switch (ipmode)
            {
                case IPMode.Loopback:

                    ip = v6 ? System.Net.IPAddress.IPv6Loopback : System.Net.IPAddress.Loopback;
                    break;
                case IPMode.Any:
                    ip = v6 ? System.Net.IPAddress.IPv6Any : System.Net.IPAddress.Any;
                    break;
                case IPMode.Specific:

                    if (!System.Net.IPAddress.TryParse(ipString, out ip))
                    {
                        throw new CassiniException(SR.ErrInvalidIPAddress, ErrorField.IPAddress);
                    }
                    break;
                default:
                    throw new CassiniException(SR.ErrInvalidIPMode, ErrorField.None);
            }
            return ip;
        }
    }
}