using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject;
using NLog;
using NzbDrone.Common;
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

        public XbmcProvider()
        {

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

                //If Dharma
                if (version == new XbmcVersion(2))
                {
                    //Check for active player only when we should skip updates when playing
                    if (!_configProvider.XbmcUpdateWhenPlaying)
                    {
                        Logger.Trace("Determining if there are any active players on XBMC host: {0}", host);
                        var activePlayers = GetActivePlayersDharma(host, username, password);

                        //If video is currently playing, then skip update
                        if(activePlayers["video"])
                        {
                            Logger.Debug("Video is currently playing, skipping library update");
                            continue;
                        }
                    }

                    UpdateWithHttp(series, host, username, password);
                }

                //If Eden or newer (attempting to make it future compatible)
                else if (version == new XbmcVersion(3) || version == new XbmcVersion(4))
                {
                    //Check for active player only when we should skip updates when playing
                    if (!_configProvider.XbmcUpdateWhenPlaying)
                    {
                        Logger.Trace("Determining if there are any active players on XBMC host: {0}", host);
                        var activePlayers = GetActivePlayersEden(host, username, password);

                        //If video is currently playing, then skip update
                        if(activePlayers.Any(a => a.Type.Equals("video")))
                        {
                            Logger.Debug("Video is currently playing, skipping library update");
                            continue;
                        }
                    }

                    UpdateWithJson(series, host, username, password);
                }

                else if (version >= new XbmcVersion(5))
                {
                    //Check for active player only when we should skip updates when playing
                    if (!_configProvider.XbmcUpdateWhenPlaying)
                    {
                        Logger.Trace("Determining if there are any active players on XBMC host: {0}", host);
                        var activePlayers = GetActivePlayersEden(host, username, password);

                        //If video is currently playing, then skip update
                        if (activePlayers.Any(a => a.Type.Equals("video")))
                        {
                            Logger.Debug("Video is currently playing, skipping library update");
                            continue;
                        }
                    }

                    UpdateWithJson(series, host, username, password);
                }

                //Log Version zero if check failed
                else
                    Logger.Trace("Unknown version: [{0}], skipping.", version);
            }
        }

        public virtual bool UpdateWithJson(Series series, string host, string username, string password)
        {
            try
            {
                //Use Json!
                var xbmcShows = GetTvShowsJson(host, username, password);

                TvShow path = null;

                //Log if response is null, otherwise try to find XBMC's path for series
                if (xbmcShows == null)
                    Logger.Trace("Failed to get TV Shows from XBMC");

                else
                    path = xbmcShows.FirstOrDefault(s => s.ImdbNumber == series.SeriesId || s.Label == series.Title);

                //var hostOnly = GetHostWithoutPort(host);

                if (path != null)
                {
                    Logger.Trace("Updating series [{0}] (Path: {1}) on XBMC host: {2}", series.Title, path.File, host);
                    //var command = String.Format("ExecBuiltIn(UpdateLibrary(video, {0}))", path.File);
                    //_eventClientProvider.SendAction(hostOnly, ActionType.ExecBuiltin, command);
                    var command = String.Format("ExecBuiltIn(UpdateLibrary(video,{0}))", path.File);
                    SendCommand(host, command, username, password);
                }

                else
                {
                    Logger.Trace("Series [{0}] doesn't exist on XBMC host: {1}, Updating Entire Library", series.Title, host);
                    var command = String.Format("ExecBuiltIn(UpdateLibrary(video))");
                    //_eventClientProvider.SendAction(hostOnly, ActionType.ExecBuiltin, command);
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

        public virtual XbmcVersion GetJsonVersion(string host, string username, string password)
        {
            //2 = Dharma
            //3 & 4 = Eden
            //5 & 6 = Frodo

            try
            {
                var postJson = new JObject();
                postJson.Add(new JProperty("jsonrpc", "2.0"));
                postJson.Add(new JProperty("method", "JSONRPC.Version"));
                postJson.Add(new JProperty("id", 10));

                var response = _httpProvider.PostCommand(host, username, password, postJson.ToString());

                if (CheckForJsonError(response))
                    return new XbmcVersion();

                Logger.Trace("Getting version from response");
                var result = JsonConvert.DeserializeObject<XbmcJsonResult<JObject>>(response);

                var versionObject = result.Result.Property("version");

                if (versionObject.Value.Type == JTokenType.Integer)
                    return new XbmcVersion((int)versionObject.Value);

                if(versionObject.Value.Type == JTokenType.Object)
                    return JsonConvert.DeserializeObject<XbmcVersion>(versionObject.Value.ToString());

                throw new InvalidCastException("Unknown Version structure!: " + versionObject);
            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
            }

            return new XbmcVersion();
        }

        public virtual Dictionary<string, bool> GetActivePlayersDharma(string host, string username, string password)
        {
            try
            {
                var postJson = new JObject();
                postJson.Add(new JProperty("jsonrpc", "2.0"));
                postJson.Add(new JProperty("method", "Player.GetActivePlayers"));
                postJson.Add(new JProperty("id", 10));

                var response = _httpProvider.PostCommand(host, username, password, postJson.ToString());

                if (CheckForJsonError(response))
                    return null;

                var result = JsonConvert.DeserializeObject<ActivePlayersDharmaResult>(response);

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
                var postJson = new JObject();
                postJson.Add(new JProperty("jsonrpc", "2.0"));
                postJson.Add(new JProperty("method", "Player.GetActivePlayers"));
                postJson.Add(new JProperty("id", 10));

                var response = _httpProvider.PostCommand(host, username, password, postJson.ToString());

                if (CheckForJsonError(response))
                    return null;

                var result = JsonConvert.DeserializeObject<ActivePlayersEdenResult>(response);

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
                var postJson = new JObject();
                postJson.Add(new JProperty("jsonrpc", "2.0"));
                postJson.Add(new JProperty("method", "VideoLibrary.GetTvShows"));
                postJson.Add(new JProperty("params", new JObject { new JProperty("properties", new string[] { "file", "imdbnumber" }) }));
                postJson.Add(new JProperty("id", 10));

                var response = _httpProvider.PostCommand(host, username, password, postJson.ToString());

                if (CheckForJsonError(response))
                    return null;

                var result = JsonConvert.DeserializeObject<TvShowResponse>(response);
                var shows = result.Result.TvShows;

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

        public virtual void TestNotification(string hosts)
        {
            foreach (var host in hosts.Split(','))
            {
                Logger.Trace("Sending Test Notifcation to XBMC Host: {0}", host);
                _eventClientProvider.SendNotification("Test Notification", "Success! Notifications are setup correctly", IconType.Jpeg, "NzbDrone.jpg", GetHostWithoutPort(host));
            }
        }

        public virtual void TestJsonApi(string hosts, string username, string password)
        {
            foreach (var host in hosts.Split(','))
            {
                Logger.Trace("Sending Test Notifcation to XBMC Host: {0}", host);
                var version = GetJsonVersion(host, username, password);
                if (version == new XbmcVersion())
                    throw new Exception("Failed to get JSON version in test");
            }
        }

        private string GetHostWithoutPort(string address)
        {
            return address.Split(':')[0];
        }
    }
}