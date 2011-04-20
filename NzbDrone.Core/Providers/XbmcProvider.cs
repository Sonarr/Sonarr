using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class XbmcProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly HttpProvider _httpProvider;

        public XbmcProvider(ConfigProvider configProvider, HttpProvider httpProvider)
        {
            _configProvider = configProvider;
            _httpProvider = httpProvider;
        }

        public virtual void Notify(string header, string message)
        {
            //Get time in seconds and convert to ms
            var time = Convert.ToInt32(_configProvider.GetValue("XbmcDisplayTime", "3", true)) * 1000;
            var command = String.Format("ExecBuiltIn(Notification({0},{1},{2}))", header, message, time);

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotificationImage", false, true)))
            {
                //Todo: Get the actual port that NzbDrone is running on...
                var serverInfo = String.Format("http://{0}:{1}", Environment.MachineName, "8989");

                var imageUrl = String.Format("{0}/Content/XbmcNotification.png", serverInfo);
                command = String.Format("ExecBuiltIn(Notification({0},{1},{2}, {3}))", header, message, time, imageUrl);
            }

            foreach (var host in _configProvider.GetValue("XbmcHosts", "localhost:80", true).Split(','))
            {
                Logger.Trace("Sending Notifcation to XBMC Host: {0}", host);
                SendCommand(host, command);
            }
        }

        public virtual void Update(int seriesId)
        {
            foreach (var host in _configProvider.GetValue("XbmcHosts", "localhost:80", true).Split(','))
            {
                Logger.Trace("Sending Update DB Request to XBMC Host: {0}", host);
                var xbmcSeriesPath = GetXbmcSeriesPath(host, seriesId);

                //If the path is not found & the user wants to update the entire library, do it now.
                if (String.IsNullOrEmpty(xbmcSeriesPath) &&
                    Convert.ToBoolean(_configProvider.GetValue("XbmcFullUpdate", false, true)))
                {
                    //Update the entire library
                    Logger.Trace("Series [{0}] doesn't exist on XBMC host: {1}, Updating Entire Library", seriesId, host);
                    SendCommand(host, "ExecBuiltIn(UpdateLibrary(video))");
                    return;
                }

                var command = String.Format("ExecBuiltIn(UpdateLibrary(video,{0}))", xbmcSeriesPath);
                SendCommand(host, command);
            }
        }

        public virtual void Clean()
        {
            foreach (var host in _configProvider.GetValue("XbmcHosts", "localhost:80", true).Split(','))
            {
                Logger.Trace("Sending DB Clean Request to XBMC Host: {0}", host);
                var command = String.Format("ExecBuiltIn(CleanLibrary(video))");
                SendCommand(host, command);
            }
        }

        private string SendCommand(string host, string command)
        {
            var username = _configProvider.GetValue("XbmcUsername", String.Empty, true);
            var password = _configProvider.GetValue("XbmcPassword", String.Empty, true);
            var url = String.Format("http://{0}/xbmcCmds/xbmcHttp?command={1}", host, command);

            if (!String.IsNullOrEmpty(username))
            {
                return _httpProvider.DownloadString(url, username, password);
            }

            return _httpProvider.DownloadString(url);
        }

        private string GetXbmcSeriesPath(string host, int seriesId)
        {
            var query =
                String.Format(
                    "select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = {0} and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath",
                    seriesId);
            var command = String.Format("QueryVideoDatabase({0})", query);

            var setResponseCommand =
                "SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            var resetResponseCommand = "SetResponseFormat()";

            SendCommand(host, setResponseCommand);
            var response = SendCommand(host, command);
            SendCommand(host, resetResponseCommand);

            if (String.IsNullOrEmpty(response))
                return String.Empty;

            var xDoc = XDocument.Load(new StringReader(response));
            var xml = (from x in xDoc.Descendants("xml") select x).FirstOrDefault();

            if (xml == null)
                return String.Empty;

            var field = xml.Descendants("field").FirstOrDefault();

            if (field == null)
                return String.Empty;

            return field.Value;
        }
    }
}