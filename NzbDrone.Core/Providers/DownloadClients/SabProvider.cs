using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Sabnzbd;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.DownloadClients
{
    public class SabProvider : IDownloadClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly HttpProvider _httpProvider;

        [Inject]
        public SabProvider(ConfigProvider configProvider, HttpProvider httpProvider)
        {
            _configProvider = configProvider;
            _httpProvider = httpProvider;
        }


        public SabProvider()
        {
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
            var queue = GetQueue().Where(c => c.ParseResult != null);

            var matchigTitle = queue.Where(q => String.Equals(q.ParseResult.CleanTitle, newParseResult.Series.CleanTitle, StringComparison.InvariantCultureIgnoreCase));

            var matchingTitleWithQuality = matchigTitle.Where(q => q.ParseResult.Quality >= newParseResult.Quality);


            if (newParseResult.Series.IsDaily)
            {
                return matchingTitleWithQuality.Any(q => q.ParseResult.AirDate.Value.Date == newParseResult.AirDate.Value.Date);
            }

            var matchingSeason = matchingTitleWithQuality.Where(q => q.ParseResult.SeasonNumber == newParseResult.SeasonNumber);

            if (newParseResult.FullSeason)
            {
                return matchingSeason.Any();
            }

            return matchingSeason.Any(q => q.ParseResult.EpisodeNumbers != null && q.ParseResult.EpisodeNumbers.Any(e => newParseResult.EpisodeNumbers.Contains(e)));
        }

        public virtual bool DownloadNzb(string url, string title)
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

            logger.Info("Adding report [{0}] to the queue.", title);

            string response = _httpProvider.DownloadString(request).Replace("\n", String.Empty);
            logger.Debug("Queue Response: [{0}]", response);

            if (response == "ok")
                return true;

            logger.Warn("SAB returned unexpected response '{0}'", response);

            return false;
        }

        public virtual List<SabQueueItem> GetQueue(int start = 0, int limit = 0)
        {
            string action = String.Format("mode=queue&output=json&start={0}&limit={1}", start, limit);
            string request = GetSabRequest(action);
            string response = _httpProvider.DownloadString(request);

            CheckForError(response);

            return JsonConvert.DeserializeObject<SabQueue>(JObject.Parse(response).SelectToken("queue").ToString()).Items;
        }

        public virtual List<SabHistoryItem> GetHistory(int start = 0, int limit = 0)
        {
            string action = String.Format("mode=history&output=json&start={0}&limit={1}", start, limit);
            string request = GetSabRequest(action);
            string response = _httpProvider.DownloadString(request);

            CheckForError(response);

            var items = JsonConvert.DeserializeObject<SabHistory>(JObject.Parse(response).SelectToken("history").ToString()).Items;
            return items ?? new List<SabHistoryItem>();
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

            if (result.Status != null && result.Status.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                throw new ApplicationException(result.Error);
        }
    }
}