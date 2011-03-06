using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NLog;

namespace NzbDrone.Core.Providers
{
    public class XbmcProvider : IXbmcProvider
    {
        private readonly IConfigProvider _configProvider;

        private WebClient _webClient;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public XbmcProvider(IConfigProvider configProvider)
        {
            _webClient = new WebClient();
            _configProvider = configProvider;
        }

        #region IXbmcProvider Members

        public void Notify(string header, string message)
        {
            //Get time in seconds and convert to ms
            var time = Convert.ToInt32(_configProvider.GetValue("XbmcDisplayTime", "3", true)) * 1000;

            var command = String.Format("ExecBuiltIn(Notification({0},{1},{2}))", header, message, time);

            foreach (var host in _configProvider.GetValue("XbmcHosts", "localhost:80", true).Split(','))
            {
                Logger.Trace("Sending Notifcation to XBMC Host: {0}", host);

            }


            throw new NotImplementedException();
        }

        public void Update(int seriesId)
        {
            throw new NotImplementedException();
        }

        public void Clean()
        {
            throw new NotImplementedException();
        }

        #endregion

        private string SendCommand(string host, string command)
        {
            var username = _configProvider.GetValue("XbmcUsername", String.Empty, true);
            var password = _configProvider.GetValue("XbmcPassword", String.Empty, true);

            if (!String.IsNullOrEmpty(username))
            {
                _webClient.Credentials = new NetworkCredential(username, password);
            }

            var url = String.Format("http://{0}/xbmcCmds/xbmcHttp?command={1}", host, command);

            try
            {
                return _webClient.DownloadString(url);
            }
            catch (Exception ex)
            {
                Logger.Warn("Unable to Connect to XBMC Host: {0}", host);
                Logger.DebugException(ex.Message, ex);
            }

            return string.Empty;
        }
    }
}
