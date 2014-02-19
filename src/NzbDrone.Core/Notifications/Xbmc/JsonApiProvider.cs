using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Newtonsoft.Json.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class JsonApiProvider : IApiProvider
    {
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public JsonApiProvider(IHttpProvider httpProvider, Logger logger)
        {
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public void Notify(XbmcSettings settings, string title, string message)
        {
            var parameters = new JObject(
                                        new JProperty("title", title),
                                        new JProperty("message", message),
                                        new JProperty("image", "https://raw.github.com/NzbDrone/NzbDrone/develop/Logo/64.png"),
                                        new JProperty("displaytime", settings.DisplayTime * 1000));

            var postJson = BuildJsonRequest("GUI.ShowNotification", parameters);

            _httpProvider.PostCommand(settings.Address, settings.Username, settings.Password, postJson.ToString());
        }

        public void Update(XbmcSettings settings, Series series)
        {
            if (!settings.AlwaysUpdate)
            {
                _logger.Trace("Determining if there are any active players on XBMC host: {0}", settings.Address);
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
            var postJson = BuildJsonRequest("VideoLibrary.Clean");

            _httpProvider.PostCommand(settings.Address, settings.Username, settings.Password, postJson.ToString());
        }

        public List<ActivePlayer> GetActivePlayers(XbmcSettings settings)
        {
            try
            {
                var postJson = new JObject();
                postJson.Add(new JProperty("jsonrpc", "2.0"));
                postJson.Add(new JProperty("method", "Player.GetActivePlayers"));
                postJson.Add(new JProperty("id", 10));

                var response = _httpProvider.PostCommand(settings.Address, settings.Username, settings.Password, postJson.ToString());

                if (CheckForError(response))
                    return new List<ActivePlayer>();

                var result = Json.Deserialize<ActivePlayersEdenResult>(response);

                return result.Result;
            }

            catch (Exception ex)
            {
                _logger.DebugException(ex.Message, ex);
            }

            return new List<ActivePlayer>();
        }

        public bool CheckForError(string response)
        {
            _logger.Trace("Looking for error in response: {0}", response);

            if (String.IsNullOrWhiteSpace(response))
            {
                _logger.Debug("Invalid response from XBMC, the response is not valid JSON");
                return true;
            }

            if (response.StartsWith("{\"error\""))
            {
                var error = Json.Deserialize<ErrorResult>(response);
                var code = error.Error["code"];
                var message = error.Error["message"];

                _logger.Debug("XBMC Json Error. Code = {0}, Message: {1}", code, message);
                return true;
            }

            return false;
        }

        public string GetSeriesPath(XbmcSettings settings, Series series)
        {
            var allSeries = GetSeries(settings);

            if (!allSeries.Any())
            {
                _logger.Trace("No TV shows returned from XBMC");
                return null;
            }

            var matchingSeries = allSeries.FirstOrDefault(s => s.ImdbNumber == series.TvdbId || s.Label == series.Title);

            if (matchingSeries != null) return matchingSeries.File;

            return null;
        }

        public bool CanHandle(XbmcVersion version)
        {
            return version >= new XbmcVersion(5);
        }

        private void UpdateLibrary(XbmcSettings settings, Series series)
        {
            try
            {
                var seriesPath = GetSeriesPath(settings, series);

                JObject postJson;

                if (seriesPath != null)
                {
                    _logger.Trace("Updating series {0} (Path: {1}) on XBMC host: {2}", series, seriesPath, settings.Address);

                    var parameters = new JObject(new JObject(new JProperty("directory", seriesPath)));
                    postJson = BuildJsonRequest("VideoLibrary.Scan", parameters);
                }

                else
                {
                    _logger.Trace("Series {0} doesn't exist on XBMC host: {1}, Updating Entire Library", series,
                                 settings.Address);

                    postJson = BuildJsonRequest("VideoLibrary.Scan");
                }

                var response = _httpProvider.PostCommand(settings.Address, settings.Username, settings.Password, postJson.ToString());

                if (CheckForError(response)) return;

                _logger.Trace(" from response");
                var result = Json.Deserialize<XbmcJsonResult<String>>(response);

                if (!result.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Trace("Failed to update library for: {0}", settings.Address);
                }
            }

            catch (Exception ex)
            {
                _logger.DebugException(ex.Message, ex);
            }
        }

        private List<TvShow> GetSeries(XbmcSettings settings)
        {
            try
            {
                var properties = new JObject { new JProperty("properties", new[] { "file", "imdbnumber" }) };
                var postJson = BuildJsonRequest("VideoLibrary.GetTvShows", properties);

                var response = _httpProvider.PostCommand(settings.Address, settings.Username, settings.Password, postJson.ToString());

                if (CheckForError(response))
                    return new List<TvShow>();

                var result = Json.Deserialize<TvShowResponse>(response);
                var shows = result.Result.TvShows;

                return shows;
            }
            catch (Exception ex)
            {
                _logger.DebugException(ex.Message, ex);
            }

            return new List<TvShow>();
        }

        private JObject BuildJsonRequest(string method)
        {
            return BuildJsonRequest(method, null);
        }

        private JObject BuildJsonRequest(string method, JObject parameters)
        {
            var postJson = new JObject();
            postJson.Add(new JProperty("jsonrpc", "2.0"));
            postJson.Add(new JProperty("method", method));

            if (parameters != null)
            {
                postJson.Add(new JProperty("params", parameters));
            }

            postJson.Add(new JProperty("id", 10));

            return postJson;
        }
    }
}
