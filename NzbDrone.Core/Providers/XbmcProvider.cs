using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model.Xbmc;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Xbmc;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class XbmcProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly HttpProvider _httpProvider;
        private readonly EventClientProvider _eventClientProvider;

        [Inject]
        public XbmcProvider(ConfigProvider configProvider, HttpProvider httpProvider, EventClientProvider eventClientProvider)
        {
            _configProvider = configProvider;
            _httpProvider = httpProvider;
            _eventClientProvider = eventClientProvider;
        }

        public virtual void Notify(string header, string message)
        {
            //Always use EventServer, until Json has real support for it
            foreach (var host in _configProvider.XbmcHosts.Split(','))
            {
                Logger.Trace("Sending Notifcation to XBMC Host: {0}", host);
                _eventClientProvider.SendNotification(header, message, IconType.Jpeg, "NzbDrone.jpg", GetHostWithoutPort(host));
            }
        }

        public XbmcProvider()
        {
            
        }

        public virtual void Update(Series series)
        {
            //Use Json for Eden/Nightly or depricated HTTP for 10.x (Dharma) to get the proper path
            //Perform update with EventServer (Json currently doesn't support updating a specific path only - July 2011)

            var username = _configProvider.XbmcUsername;
            var password = _configProvider.XbmcPassword;

            foreach (var host in _configProvider.XbmcHosts.Split(','))
            {
                Logger.Trace("Determining version of XBMC Host: {0}", host);
                var version = GetJsonVersion(host, username, password);

                Logger.Trace("No video playing, proceeding with library update");

                //If Dharma
                if (version == 2)
                {
                    Logger.Trace("Determining if there are any active players on XBMC host: {0}", host);
                    var activePlayers = GetActivePlayersDharma(host, username, password);

                    //If video is currently playing, then skip update
                    if (activePlayers["video"])
                    {
                        Logger.Debug("Video is currently playing, skipping library update");
                        continue;
                    }

                    UpdateWithHttp(series, host, username, password);
                }

                //If Eden or newer (attempting to make it future compatible)
                else if (version >= 3)
                {
                    Logger.Trace("Determining if there are any active players on XBMC host: {0}", host);
                    var activePlayers = GetActivePlayersEden(host, username, password);

                    //If video is currently playing, then skip update
                    if (activePlayers.Any(a => a.Type.Equals("video")))
                    {
                        Logger.Debug("Video is currently playing, skipping library update");
                        continue;
                    }

                    UpdateWithJson(series, password, host, username);
                }
            }
        }

        public virtual bool UpdateWithJson(Series series, string host, string username, string password)
        {
            try
            {
                //Use Json!
                var xbmcShows = GetTvShowsJson(host, username, password);
                var path = xbmcShows.Where(s => s.ImdbNumber == series.SeriesId || s.Label == series.Title).FirstOrDefault();

                var hostOnly = GetHostWithoutPort(host);

                if (path != null)
                {
                    Logger.Trace("Updating series [{0}] on XBMC host: {1}", series.Title, host);
                    var command = String.Format("ExecBuiltIn(UpdateLibrary(video,{0}))", path.File);
                    _eventClientProvider.SendAction(hostOnly, ActionType.ExecBuiltin, command);
                }

                else
                {
                    Logger.Trace("Series [{0}] doesn't exist on XBMC host: {1}, Updating Entire Library", series.Title, host);
                    var command = String.Format("ExecBuiltIn(UpdateLibrary(video))");
                    _eventClientProvider.SendAction(hostOnly, ActionType.ExecBuiltin, command);
                }
            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
                return false;
            }

            return true;
        }

        public virtual bool UpdateWithHttp(Series series, string host, string username, string password)
        {
            try
            {
                Logger.Trace("Sending Update DB Request to XBMC Host: {0}", host);
                var xbmcSeriesPath = GetXbmcSeriesPath(host, series.SeriesId, username, password);

                //If the path is found update it, else update the whole library
                if (!String.IsNullOrEmpty(xbmcSeriesPath))
                {
                    Logger.Trace("Updating series [{0}] on XBMC host: {1}", series.Title, host);
                    var command = String.Format("ExecBuiltIn(UpdateLibrary(video,{0}))", xbmcSeriesPath);
                    SendCommand(host, command, username, password);
                }

                else
                {
                    //Update the entire library
                    Logger.Trace("Series [{0}] doesn't exist on XBMC host: {1}, Updating Entire Library", series.Title, host);
                    SendCommand(host, "ExecBuiltIn(UpdateLibrary(video))", username, password);
                }
            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
                return false;
            }
            
            return true;
        }

        public virtual void Clean()
        {
            //Use EventServer, once Dharma is extinct use Json?

            foreach (var host in _configProvider.XbmcHosts.Split(','))
            {
                Logger.Trace("Sending DB Clean Request to XBMC Host: {0}", host);
                var command = "ExecBuiltIn(CleanLibrary(video))";
                _eventClientProvider.SendAction(GetHostWithoutPort(host), ActionType.ExecBuiltin, command);
            }
        }

        public virtual string SendCommand(string host, string command, string username, string password)
        {
            var url = String.Format("http://{0}/xbmcCmds/xbmcHttp?command={1}", host, command);

            if (!String.IsNullOrEmpty(username))
            {
                return _httpProvider.DownloadString(url, username, password);
            }

            return _httpProvider.DownloadString(url);
        }

        public virtual string GetXbmcSeriesPath(string host, int seriesId, string username, string password)
        {
            var query =
                String.Format(
                    "select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = {0} and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath",
                    seriesId);
            var command = String.Format("QueryVideoDatabase({0})", query);

            const string setResponseCommand =
                "SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            const string resetResponseCommand = "SetResponseFormat()";

            SendCommand(host, setResponseCommand, username, password);
            var response = SendCommand(host, command, username, password);
            SendCommand(host, resetResponseCommand, username, password);

            if (String.IsNullOrEmpty(response))
                return String.Empty;

            var xDoc = XDocument.Load(new StringReader(response.Replace("&", "&amp;")));
            var xml = (from x in xDoc.Descendants("xml") select x).FirstOrDefault();

            if (xml == null)
                return String.Empty;

            var field = xml.Descendants("field").FirstOrDefault();

            if (field == null)
                return String.Empty;

            return field.Value;
        }

        public virtual int GetJsonVersion(string host, string username, string password)
        {
            //2 = Dharma
            //3 = Eden/Nightly (as of July 2011)

            var version = 0;

            try
            {
                var command = new Command { id = 10, method = "JSONRPC.Version" };
                var serializer = new JavaScriptSerializer();
                var serialized = serializer.Serialize(command);
                var response = _httpProvider.PostCommand(host, username, password, serialized);

                if (CheckForJsonError(response))
                    return version;

                Logger.Trace("Getting version from response");
                var result = serializer.Deserialize<VersionResult>(response);
                result.Result.TryGetValue("version", out version);
            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
            }

            return version;
        }

        public virtual Dictionary<string, bool> GetActivePlayersDharma(string host, string username, string password)
        {
            try
            {
                var command = new Command { id = 10, method = "Player.GetActivePlayers" };
                var serializer = new JavaScriptSerializer();
                var serialized = serializer.Serialize(command);
                var response = _httpProvider.PostCommand(host, username, password, serialized);

                if (CheckForJsonError(response))
                    return null;

                var result = serializer.Deserialize<ActivePlayersDharmaResult>(response);

                return result.Result;
            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
            }

            return null;
        }

        public virtual List<ActivePlayer> GetActivePlayersEden(string host, string username, string password)
        {
            try
            {
                var command = new Command { id = 10, method = "Player.GetActivePlayers" };
                var serializer = new JavaScriptSerializer();
                var serialized = serializer.Serialize(command);
                var response = _httpProvider.PostCommand(host, username, password, serialized);

                if (CheckForJsonError(response))
                    return null;

                var result = serializer.Deserialize<ActivePlayersEdenResult>(response);

                return result.Result;
            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
            }

            return null;
        }

        public virtual List<TvShow> GetTvShowsJson(string host, string username, string password)
        {
            try
            {
                var fields = new string[] { "file", "imdbnumber" };
                var xbmcParams = new Params { fields = fields };
                var command = new Command { id = 10, method = "VideoLibrary.GetTvShows", @params = xbmcParams };
                var serializer = new JavaScriptSerializer();
                var serialized = serializer.Serialize(command);
                var response = _httpProvider.PostCommand(host, username, password, serialized);

                if (CheckForJsonError(response))
                    return null;

                var result = serializer.Deserialize<TvShowResult>(response);
                var shows = result.Result["tvshows"];

                return shows;
            }
            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
            }
            return null;
        }

        public virtual bool CheckForJsonError(string response)
        {
            Logger.Trace("Looking for error in response: {0}", response);

            if (response.StartsWith("{\"error\""))
            {
                var serializer = new JavaScriptSerializer();
                var error = serializer.Deserialize<ErrorResult>(response);
                var code = error.Error["code"];
                var message = error.Error["message"];

                Logger.Debug("XBMC Json Error. Code = {0}, Message: {1}", code, message);
                return true;
            }

            if (String.IsNullOrWhiteSpace(response))
            {
                Logger.Debug("Invalid response from XBMC, the response is not valid JSON");
                return true;
            }

            return false;
        }

        private string GetHostWithoutPort(string address)
        {
            return address.Split(':')[0];
        }
    }
}