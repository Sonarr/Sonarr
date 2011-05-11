using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class AutoConfigureProvider
    {
        public AutoConfigureProvider()
        {
        }

        public SabnzbdInfoModel AutoConfigureSab()
        {
            var info = GetConnectionList();
            return FindApiKey(info);
        }

        private List<ConnectionInfoModel> GetConnectionList()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            var info =
                ipProperties.GetActiveTcpListeners().Select(
                    p =>
                    new ConnectionInfoModel
                        {Address = p.Address.ToString().Replace("0.0.0.0", "127.0.0.1"), Port = p.Port}).Distinct().
                    ToList();

            info.RemoveAll(i => i.Port == 135);
            info.RemoveAll(i => i.Port == 139);
            info.RemoveAll(i => i.Port == 445);
            info.RemoveAll(i => i.Port == 3389);
            info.RemoveAll(i => i.Port == 5900);
            info.RemoveAll(i => i.Address.Contains("::"));

            info.Reverse();

            return info;
        }

        private SabnzbdInfoModel FindApiKey(List<ConnectionInfoModel> info)
        {
            foreach (var connection in info)
            {
                var apiKey = GetApiKey(connection.Address, connection.Port);
                if (!String.IsNullOrEmpty(apiKey))
                    return new SabnzbdInfoModel {
                        Host = connection.Address,
                        Port = connection.Port,
                        ApiKey = apiKey
                    };
            }
            return null;
        }

        private string GetApiKey(string ipAddress, int port)
        {
            var request = String.Format("http://{0}:{1}/config/general/", ipAddress, port);
            var result = DownloadString(request);

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

        private string DownloadString(string url)
        {
            try
            {
                var request = WebRequest.Create(url);
                request.Timeout = 2000;

                var response = request.GetResponse();

                var reader = new StreamReader(response.GetResponseStream());
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get response from: {0}", url);
                Console.WriteLine(ex.Message, ex);
            }

            return String.Empty;
        }
    }
}
