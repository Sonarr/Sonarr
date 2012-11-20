using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Sabnzbd;
using System.Diagnostics;
using NzbDrone.Core.Helpers;


namespace NzbDrone.Core.Providers
{
    public class AutoConfigureProvider
    {
        private const string sabModuleName = "SABNZBD.EXE";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SabModel AutoConfigureSab(string host, int port, string username, string password)
        {
            var sabProcess = FindLocalSabProcess();
            if (sabProcess != null /* Not sure yet: && (IsLocalIpAddress(host) || string.IsNullOrEmpty(host))*/)       // The extra IsLocalIpAddress is there in case the user has sab running on multiple machines, including localhost.
            {
                sabProcess.Credentials = FindLocalSabCredentials();

                if (sabProcess.Credentials == null)
                {
                    sabProcess.Credentials = new CredentialInfoModel { Username = username, Password = password };
                }

                return FindApiKey(new List<ConnectionInfoModel>() { sabProcess });
            }
            else
            {
                return FindApiKey(new List<ConnectionInfoModel>()
                { new ConnectionInfoModel { Address = host, Port = port, Credentials = new CredentialInfoModel { Username = username, Password = password }}}
                );
            }
        }

        private ConnectionInfoModel FindLocalSabProcess()
        {
            foreach (TcpRow tcpRow in IpHelper.GetExtendedTcpTable(true))
            {
                try
                {
                    if (Process.GetProcessById(tcpRow.ProcessId).MainModule.ModuleName.ToUpper() == sabModuleName)
                    {
                        return new ConnectionInfoModel { Address = tcpRow.LocalEndPoint.Address.ToString(), Port = tcpRow.LocalEndPoint.Port };
                    }
                }
                catch { }
            }
            return null;
        }

        private CredentialInfoModel FindLocalSabCredentials()
        {
            var sabPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"sabnzbd\sabnzbd.ini");
            if (File.Exists(sabPath) == false)
                return null;

            var lines = File.ReadAllLines(sabPath).ToList();
            var userLine = lines.Find(line => line.Replace(" ", "").Contains("username="));
            var passLine = lines.Find(line => line.Replace(" ", "").Contains("password="));

            if (string.IsNullOrEmpty(userLine) == false && string.IsNullOrEmpty(passLine) == false)
            {
                var uArr = userLine.Split('=');
                var pArr = passLine.Split('=');

                if (uArr.Length == 2 && pArr.Length == 2)
                {
                    return new CredentialInfoModel { Username = uArr[1].Trim(), Password = pArr[1].Trim() };
                }
            }
            return null;
        }

        public static bool IsLocalIpAddress(string host)
        {
            try
            { 
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                foreach (IPAddress hostIP in hostIPs)
                {
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private SabModel FindApiKey(List<ConnectionInfoModel> info)
        {
            foreach (var connection in info)
            {
                Debug.WriteLine(string.Format("{0}:{1}, {2};{3}", connection.Address, connection.Port, connection.Credentials.Username, connection.Credentials.Password));
                var apiKey = GetApiKey(connection);
                if (!String.IsNullOrEmpty(apiKey))
                    return new SabModel
                    {
                        Host = connection.Address,
                        Port = connection.Port,
                        ApiKey = apiKey,
                        Username = connection.HasCredentials ? connection.Credentials.Username : "",
                        Password = connection.HasCredentials ? connection.Credentials.Password : ""
                    };
            }
            return null;
        }

        private string GetApiKey(ConnectionInfoModel connectionInfo)
        {
            var request = String.Format("http://{0}:{1}/config/general/", connectionInfo.Address, connectionInfo.Port);
            var result = DownloadString(connectionInfo, request);

            Regex regex =
                new Regex("\\<input\\Wtype\\=\\\"text\\\"\\Wid\\=\\\"apikey\\\"\\Wvalue\\=\\\"(?<apikey>\\w+)\\W",
                          RegexOptions.IgnoreCase
                          | RegexOptions.Compiled);
            var match = regex.Match(result);

            if (match.Success)
            {
                return match.Groups["apikey"].Value;
            }

            return String.Empty;
        }

        private string DownloadString(ConnectionInfoModel connectionInfo, string url)
        {
            try
            {
                var request = WebRequest.Create(url);
                if (connectionInfo.HasCredentials)
                {
                    request.Credentials = CreateBasicAuthCredential(connectionInfo, url);
                    request.PreAuthenticate = true;
                }
                request.Timeout = 2000;

                var response = request.GetResponse();

                var reader = new StreamReader(response.GetResponseStream());
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logger.Trace("Failed to get response from: {0}", url);
                Logger.Trace(ex.Message, ex);
            }

            return String.Empty;
        }

        private CredentialCache CreateBasicAuthCredential(ConnectionInfoModel connectionInfo, string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            CredentialCache credentialCache = new CredentialCache();
            credentialCache.Add(new System.Uri(url), "Basic", new NetworkCredential(connectionInfo.Credentials.Username, connectionInfo.Credentials.Password));
            return credentialCache;
        }
    }
}
