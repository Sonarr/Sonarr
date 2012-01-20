using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Sabnzbd;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class SabProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly HttpProvider _httpProvider;

        public SabProvider()
        {
        }

        [Inject]
        public SabProvider(ConfigProvider configProvider, HttpProvider httpProvider)
        {
            _configProvider = configProvider;
            _httpProvider = httpProvider;
        }

        public virtual bool AddByUrl(string url, string title)
        {
            string cat = _configProvider.SabTvCategory;
            int priority = (int)_configProvider.SabTvPriority;
            string name = GetNzbName(url);
            string nzbName = HttpUtility.UrlEncode(title);

            string action = string.Format("mode=addurl&name={0}&priority={1}&pp=3&cat={2}&nzbname={3}",
                name, priority, cat, nzbName);

            if (url.ToLower().Contains("newzbin"))
            {
                action = action.Replace("mode=addurl", "mode=addid");
            }

            string request = GetSabRequest(action);

            Logger.Info("Adding report [{0}] to the queue.", title);

            string response = _httpProvider.DownloadString(request).Replace("\n", String.Empty);
            Logger.Debug("Queue Response: [{0}]", response);

            if (response == "ok")
                return true;

            Logger.Warn("SAB returned unexpected response '{0}'", response);

            return false;
        }

        private static string GetNzbName(string urlString)
        {
            var url = new Uri(urlString);
            if (url.Host.ToLower().Contains("newzbin"))
            {
                var postId = Regex.Match(urlString, @"\d{5,10}").Value;
                return postId;
            }

            return urlString.Replace("&", "%26");
        }

        public virtual bool IsInQueue(EpisodeParseResult newParseResult)
        {
            var queue = GetQueue();

            return queue.Any(c => String.Equals(c.ParseResult.SeriesTitle, newParseResult.Series.Title, StringComparison.InvariantCultureIgnoreCase) &&
                                  c.ParseResult.EpisodeNumbers.Any(e=> newParseResult.EpisodeNumbers.Contains(e))&&
                                  c.ParseResult.SeasonNumber == newParseResult.SeasonNumber &&
                                  c.ParseResult.Quality >= newParseResult.Quality);
        }

        public virtual List<SabQueueItem> GetQueue(int start = 0, int limit = 0)
        {
            string action = String.Format("mode=queue&output=json&start={0}&limit={1}", start, limit);
            string request = GetSabRequest(action);
            string response = _httpProvider.DownloadString(request);

            CheckForError(response);

            return JsonConvert.DeserializeObject<SabQueue>(response).Items;
        }

        public virtual List<SabHistoryItem> GetHistory(int start = 0, int limit = 0)
        {
            string action = String.Format("mode=history&output=json&start={0}&limit={1}", start, limit);
            string request = GetSabRequest(action);
            string response = _httpProvider.DownloadString(request);

            CheckForError(response);

            return JsonConvert.DeserializeObject<SabHistory>(response).Items;
        }

        public virtual String GetSabTitle(EpisodeParseResult parseResult)
        {
            //Handle Full Naming
            if (parseResult.FullSeason)
            {
                var seasonResult = String.Format("{0} - Season {1} [{2}]", GetSabSeriesName(parseResult),
                                     parseResult.SeasonNumber, parseResult.Quality.QualityType);

                if (parseResult.Quality.Proper)
                    seasonResult += " [Proper]";

                return seasonResult;
            }

            if (parseResult.Series.IsDaily)
            {
                var dailyResult = String.Format("{0} - {1:yyyy-MM-dd} - {2} [{3}]", GetSabSeriesName(parseResult),
                                     parseResult.AirDate, parseResult.EpisodeTitle, parseResult.Quality.QualityType);

                if (parseResult.Quality.Proper)
                    dailyResult += " [Proper]";

                return dailyResult;
            }

            //Show Name - 1x01-1x02 - Episode Name
            //Show Name - 1x01 - Episode Name
            var episodeString = new List<String>();

            foreach (var episode in parseResult.EpisodeNumbers)
            {
                episodeString.Add(String.Format("{0}x{1}", parseResult.SeasonNumber, episode));
            }

            var epNumberString = String.Join("-", episodeString);

            var result = String.Format("{0} - {1} - {2} [{3}]", GetSabSeriesName(parseResult), epNumberString, parseResult.EpisodeTitle, parseResult.Quality.QualityType);

            if (parseResult.Quality.Proper)
            {
                result += " [Proper]";
            }

            return result;
        }

        private static string GetSabSeriesName(EpisodeParseResult parseResult)
        {
            return MediaFileProvider.CleanFilename(parseResult.Series.Title);
        }

        public virtual SabCategoryModel GetCategories(string host = null, int port = 0, string apiKey = null, string username = null, string password = null)
        {
            //Get saved values if any of these are defaults
            if (host == null)
                host = _configProvider.SabHost;

            if (port == 0)
                port = _configProvider.SabPort;

            if (apiKey == null)
                apiKey = _configProvider.SabApiKey;

            if (username == null)
                username = _configProvider.SabUsername;

            if (password == null)
                password = _configProvider.SabPassword;

            const string action = "mode=get_cats&output=json";

            var command = string.Format(@"http://{0}:{1}/api?{2}&apikey={3}&ma_username={4}&ma_password={5}",
                                 host, port, action, apiKey, username, password);

            var response = _httpProvider.DownloadString(command);

            if (String.IsNullOrWhiteSpace(response))
                return new SabCategoryModel { categories = new List<string>() };

            var categories = JsonConvert.DeserializeObject<SabCategoryModel>(response);

            return categories;
        }

        private string GetSabRequest(string action)
        {
            return string.Format(@"http://{0}:{1}/api?{2}&apikey={3}&ma_username={4}&ma_password={5}",
                                 _configProvider.SabHost,
                                 _configProvider.SabPort,
                                 action,
                                 _configProvider.SabApiKey,
                                 _configProvider.SabUsername,
                                 _configProvider.SabPassword);
        }

        private void CheckForError(string response)
        {
            var result = JsonConvert.DeserializeObject<SabJsonError>(response);

            if (result.Status.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                throw new ApplicationException(result.Error);
        }
    }
}