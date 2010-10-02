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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;

#endregion

namespace CassiniDev
{
    public static class CassiniNetworkUtils
    {
        public static IPAddress[] GetLocalAddresses()
        {
            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            return ipEntry.AddressList;
        }

        /// <summary>
        /// Returns first available port on the specified IP address. 
        /// The port scan excludes ports that are open on ANY loopback adapter. 
        /// 
        /// If the address upon which a port is requested is an 'ANY' address all 
        /// ports that are open on ANY IP are excluded.
        /// </summary>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        /// <param name="ip">The IP address upon which to search for available port.</param>
        /// <param name="includeIdlePorts">If true includes ports in TIME_WAIT state in results. 
        /// TIME_WAIT state is typically cool down period for recently released ports.</param>
        /// <returns></returns>
        public static int GetAvailablePort(int rangeStart, int rangeEnd, IPAddress ip, bool includeIdlePorts)
        {
            IPGlobalProperties ipProps = IPGlobalProperties.GetIPGlobalProperties();

            // if the ip we want a port on is an 'any' or loopback port we need to exclude all ports that are active on any IP
            Func<IPAddress, bool> isIpAnyOrLoopBack = i => IPAddress.Any.Equals(i) ||
                                                           IPAddress.IPv6Any.Equals(i) ||
                                                           IPAddress.Loopback.Equals(i) ||
                                                           IPAddress.IPv6Loopback.
                                                               Equals(i);
            // get all active ports on specified IP. 
            List<ushort> excludedPorts = new List<ushort>();

            // if a port is open on an 'any' or 'loopback' interface then include it in the excludedPorts
            excludedPorts.AddRange(from n in ipProps.GetActiveTcpConnections()
                                   where
                                       n.LocalEndPoint.Port >= rangeStart &&
                                       n.LocalEndPoint.Port <= rangeEnd && (
                                                                               isIpAnyOrLoopBack(ip) ||
                                                                               n.LocalEndPoint.Address.Equals(ip) ||
                                                                               isIpAnyOrLoopBack(n.LocalEndPoint.Address)) &&
                                       (!includeIdlePorts || n.State != TcpState.TimeWait)
                                   select (ushort) n.LocalEndPoint.Port);

            excludedPorts.AddRange(from n in ipProps.GetActiveTcpListeners()
                                   where n.Port >= rangeStart && n.Port <= rangeEnd && (
                                                                                           isIpAnyOrLoopBack(ip) ||
                                                                                           n.Address.Equals(ip) ||
                                                                                           isIpAnyOrLoopBack(n.Address))
                                   select (ushort) n.Port);

            excludedPorts.AddRange(from n in ipProps.GetActiveUdpListeners()
                                   where n.Port >= rangeStart && n.Port <= rangeEnd && (
                                                                                           isIpAnyOrLoopBack(ip) ||
                                                                                           n.Address.Equals(ip) ||
                                                                                           isIpAnyOrLoopBack(n.Address))
                                   select (ushort) n.Port);

            excludedPorts.Sort();

            for (int port = rangeStart; port <= rangeEnd; port++)
            {
                if (!excludedPorts.Contains((ushort) port))
                {
                    return port;
                }
            }

            return 0;
        }

        ///<summary>
        /// Returns the first IPV4 address available for this host.
        /// This is typically an external IP
        ///</summary>
        ///<returns></returns>
        public static IPAddress GetExternalIPV4()
        {
            return GetIPAdresses().ToList()
                .FirstOrDefault(i => i.ToString().IndexOf(":") == -1);
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }

        ///<summary>
        /// Gets all IP addresses for this host
        ///</summary>
        public static IPAddress[] GetIPAdresses()
        {
            return Dns.GetHostEntry(GetHostName()).AddressList;
        }

        /// <summary>
        /// Gently polls specified IP:Port to determine if it is available.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public static bool IsPortAvailable(IPAddress ipAddress, int port)
        {
            bool portAvailable = false;

            for (int i = 0; i < 5; i++)
            {
                portAvailable = GetAvailablePort(port, port, ipAddress, true) == port;
                if (portAvailable)
                {
                    break;
                }
                // be a little patient and wait for the port if necessary,
                // the previous occupant may have just vacated
                Thread.Sleep(100);
            }
            return portAvailable;
        }

        /// <summary>
        /// Combine the RootUrl of the running web application with the relative url
        /// specified.
        /// </summary>
        /// <param name="rootUrl"></param>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public static string NormalizeUrl(string rootUrl, string relativeUrl)
        {
            relativeUrl = relativeUrl.TrimStart(new[] {'/'});

            if (!rootUrl.EndsWith("/"))
            {
                rootUrl += "/";
            }
            return new Uri(rootUrl + relativeUrl).ToString();
        }

        ///<summary>
        ///</summary>
        ///<param name="ipString"></param>
        ///<returns></returns>
        public static IPAddress ParseIPString(string ipString)
        {
            if (string.IsNullOrEmpty(ipString))
            {
                ipString = "loopback";
            }
            ipString = ipString.Trim().ToLower();
            switch (ipString)
            {
                case "any":
                    return IPAddress.Any;
                case "loopback":
                    return IPAddress.Loopback;
                case "ipv6any":
                    return IPAddress.IPv6Any;
                case "ipv6loopback":
                    return IPAddress.IPv6Loopback;
                default:
                    IPAddress result;
                    IPAddress.TryParse(ipString, out result);
                    return result;
            }
        }

        /// <summary>
        /// <para>
        /// Hostnames are composed of series of labels concatenated with dots, as are all domain names[1]. 
        /// For example, "en.wikipedia.org" is a hostname. Each label must be between 1 and 63 characters long, 
        /// and the entire hostname has a maximum of 255 characters.</para>
        /// <para>
        /// The Internet standards (Request for Comments) for protocols mandate that component hostname 
        /// labels may contain only the ASCII letters 'a' through 'z' (in a case-insensitive manner), the digits 
        /// '0' through '9', and the hyphen. The original specification of hostnames in RFC 952, mandated that 
        /// labels could not start with a digit or with a hyphen, and must not end with a hyphen. However, a 
        /// subsequent specification (RFC 1123) permitted hostname labels to start with digits. No other symbols, 
        /// punctuation characters, or blank spaces are permitted.</para>
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        /// http://en.wikipedia.org/wiki/Hostname#Restrictions_on_valid_host_names
        public static bool ValidateHostName(string hostname)
        {
            Regex hostnameRx =
                new Regex(
                    @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\-]*[A-Za-z0-9])$");
            return hostnameRx.IsMatch(hostname);
        }
    }
}