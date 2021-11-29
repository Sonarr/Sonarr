using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class HttpApiProvider : IApiProvider
    {
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public HttpApiProvider(IHttpProvider httpProvider, Logger logger)
        {
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public bool CanHandle(XbmcVersion version)
        {
            return version < new XbmcVersion(5);
        }

        public void Notify(XbmcSettings settings, string title, string message)
        {
            var notification = string.Format("Notification({0},{1},{2},{3})", title, message, settings.DisplayTime * 1000, "https://raw.github.com/Sonarr/Sonarr/develop/Logo/64.png");
            var command = BuildExecBuiltInCommand(notification);

            SendCommand(settings, command);
        }

        public void Update(XbmcSettings settings, Series series)
        {
            if (!settings.AlwaysUpdate)
            {
                _logger.Debug("Determining if there are any active players on XBMC host: {0}", settings.Address);
                var activePlayers = GetActivePlayers(settings);

                if (activePlayers.Any(a => a.Type.Equals("video")))
                {
                    _logger.Debug("Video is currently playing, skipping library update");
                    return;
                }
            }

            UpdateLibrary(settings, series);
        }

        public void Clean(XbmcSettings settings)
        {
            if (!settings.AlwaysUpdate)
            {
                _logger.Debug("Determining if there are any active players on XBMC host: {0}", settings.Address);
                var activePlayers = GetActivePlayers(settings);

                if (activePlayers.Any(a => a.Type.Equals("video")))
                {
                    _logger.Debug("Video is currently playing, skipping library cleaning");
                    return;
                }
            }

            const string cleanVideoLibrary = "CleanLibrary(video)";
            var command = BuildExecBuiltInCommand(cleanVideoLibrary);

            SendCommand(settings, command);
        }

        internal List<ActivePlayer> GetActivePlayers(XbmcSettings settings)
        {
            try
            {
                var result = new List<ActivePlayer>();
                var response = SendCommand(settings, "getcurrentlyplaying");

                if (response.Contains("<li>Filename:[Nothing Playing]")) return new List<ActivePlayer>();
                if (response.Contains("<li>Type:Video")) result.Add(new ActivePlayer(1, "video"));

                return result;
            }

            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
            }

            return new List<ActivePlayer>();
        }
        
        internal string GetSeriesPath(XbmcSettings settings, Series series)
        {
            var query =
                string.Format(
                    "select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = {0} and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath",
                    series.TvdbId);
            var command = string.Format("QueryVideoDatabase({0})", query);

            const string setResponseCommand =
                "SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            const string resetResponseCommand = "SetResponseFormat()";

            SendCommand(settings, setResponseCommand);
            var response = SendCommand(settings, command);
            SendCommand(settings, resetResponseCommand);

            if (string.IsNullOrEmpty(response))
                return string.Empty;

            var xDoc = XDocument.Load(new StringReader(response.Replace("&", "&amp;")));
            var xml = xDoc.Descendants("xml").Select(x => x).FirstOrDefault();

            if (xml == null)
                return null;

            var field = xml.Descendants("field").FirstOrDefault();

            if (field == null)
                return null;

            return field.Value;
        }

        internal bool CheckForError(string response)
        {
            _logger.Debug("Looking for error in response: {0}", response);

            if (string.IsNullOrWhiteSpace(response))
            {
                _logger.Debug("Invalid response from XBMC, the response is not valid JSON");
                return true;
            }

            var errorIndex = response.IndexOf("Error", StringComparison.InvariantCultureIgnoreCase);

            if (errorIndex > -1)
            {
                var errorMessage = response.Substring(errorIndex + 6);
                errorMessage = errorMessage.Substring(0, errorMessage.IndexOfAny(new char[] { '<', ';' }));

                _logger.Debug("Error found in response: {0}", errorMessage);
                return true;
            }

            return false;
        }

        private void UpdateLibrary(XbmcSettings settings, Series series)
        {
            try
            {
                _logger.Debug("Sending Update DB Request to XBMC Host: {0}", settings.Address);
                var xbmcSeriesPath = GetSeriesPath(settings, series);

                //If the path is found update it, else update the whole library
                if (!string.IsNullOrEmpty(xbmcSeriesPath))
                {
                    _logger.Debug("Updating series [{0}] on XBMC host: {1}", series, settings.Address);
                    var command = BuildExecBuiltInCommand(string.Format("UpdateLibrary(video,{0})", xbmcSeriesPath));
                    SendCommand(settings, command);
                }

                else
                {
                    //Update the entire library
                    _logger.Debug("Series [{0}] doesn't exist on XBMC host: {1}, Updating Entire Library", series, settings.Address);
                    var command = BuildExecBuiltInCommand("UpdateLibrary(video)");
                    SendCommand(settings, command);
                }
            }

            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
            }
        }

        private string SendCommand(XbmcSettings settings, string command)
        {
            var url = $"{settings.Address}/xbmcCmds/xbmcHttp?command={command}";

            if (!string.IsNullOrEmpty(settings.Username))
            {
                return _httpProvider.DownloadString(url, settings.Username, settings.Password);
            }

            return _httpProvider.DownloadString(url);
        }

        private string BuildExecBuiltInCommand(string command)
        {
            return string.Format("ExecBuiltIn({0})", command);
        }
    }
}
