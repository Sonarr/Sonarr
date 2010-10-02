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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

#endregion

namespace CassiniDev
{
    public static class HostsFile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static int AddHostEntry(string ipAddress, string hostname)
        {
            try
            {
                SetHostsEntry(true, ipAddress, hostname);
                return 0;
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }
            string executablePath = Assembly.GetExecutingAssembly().Location;
            return StartElevated(executablePath, string.Format("Hostsfile /ah+ /h:{0} /i:{1}", hostname, ipAddress));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static int RemoveHostEntry(string ipAddress, string hostname)
        {
            try
            {
                SetHostsEntry(false, ipAddress, hostname);
                return 0;
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }
            string executablePath = Assembly.GetExecutingAssembly().Location;
            return StartElevated(executablePath, string.Format("Hostsfile /ah- /h:{0} /i:{1}", hostname, ipAddress));
        }

        private static void SetHostsEntry(bool addHost, string ipAddress, string hostname)
        {
            // limitation: while windows allows mulitple entries for a single host, we currently allow only one
            string windir = Environment.GetEnvironmentVariable("SystemRoot") ?? @"c:\windows";
            string hostsFilePath = Path.Combine(windir, @"system32\drivers\etc\hosts");

            string hostsFileContent = File.ReadAllText(hostsFilePath);

            hostsFileContent = Regex.Replace(hostsFileContent,
                                             string.Format(@"\r\n^\s*[\d\w\.:]+\s{0}\s#\sadded\sby\scassini$",
                                                           hostname), "", RegexOptions.Multiline);

            if (addHost)
            {
                hostsFileContent += string.Format("\r\n{0} {1} # added by cassini", ipAddress, hostname);
            }

            File.WriteAllText(hostsFilePath, hostsFileContent);
        }

        private static int StartElevated(string filename, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = filename,
                    Arguments = args,
                    Verb = "runas"
                };
            try
            {
                Process p = Process.Start(startInfo);
                if (p != null)
                {
                    p.WaitForExit();
                    return p.ExitCode;
                }
                return -2;
            }
            catch
            {
                return -2;
            }
        }
    }
}