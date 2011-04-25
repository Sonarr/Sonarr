using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class AutoConfigureProvider
    {
        private HttpProvider _httpProvider;
        private ConfigProvider _configProvider;

        public AutoConfigureProvider(HttpProvider httpProvider, ConfigProvider configProvider)
        {
            _httpProvider = httpProvider;
            _configProvider = configProvider;
        }

        public SabnzbdInfoModel AutoConfigureSab(string username, string password)
        {
            //Get Output from Netstat
            var netStatOutput = String.Empty;
            //var port = GetSabnzbdPort(netStatOutput);
            var port = 2222;
            var apiKey = GetSabnzbdApiKey(port);

            if (port > 0 && !String.IsNullOrEmpty(apiKey))
            {
                return new SabnzbdInfoModel
                           {
                               ApiKey = apiKey,
                               Port = port,
                               Username = username,
                               Password = password
                           };
            }

            return null;
        }

        private int GetSabnzbdPort(string netstatOutput)
        {
            Regex regex = new Regex(@"^(?:TCP\W+127.0.0.1:(?<port>\d+\W+).+?\r\n\W+\[sabnzbd.exe\])", RegexOptions.IgnoreCase
                                                                                | RegexOptions.Compiled);
            var match = regex.Match(netstatOutput);
            var port = 0;
            Int32.TryParse(match.Groups["port"].Value, out port);

            return port;
        }

        private string GetSabnzbdApiKey(int port, string ipAddress = "127.0.0.1")
        {
            var request = String.Format("http://{0}:{1}/config/general/", ipAddress, port);
            var result = _httpProvider.DownloadString(request);

            Regex regex = new Regex("\\<input\\Wtype\\=\\\"text\\\"\\Wid\\=\\\"apikey\\\"\\Wvalue\\=\\\"(?<apikey>\\w+)\\W", RegexOptions.IgnoreCase
                                                                                | RegexOptions.Compiled);
            var match = regex.Match(result);

            return match.Groups["apikey"].Value;
        }
    }
}
