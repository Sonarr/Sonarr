using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Sabnzbd;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Providers.DownloadClients
{
    public class SabProvider : IDownloadClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfigService _configService;
        private readonly HttpProvider _httpProvider;

        public SabProvider(IConfigService configService, HttpProvider httpProvider)
        {
            _configService = configService;
            _httpProvider = httpProvider;
        }

        public SabProvider()
        {
        }

        public virtual bool IsInQueue(EpisodeParseResult newParseResult)
        {
            try
            {
                var queue = GetQueue().Where(c => c.ParseResult != null);

                var matchigTitle = queue.Where(q => String.Equals(q.ParseResult.CleanTitle, newParseResult.Series.CleanTitle, StringComparison.InvariantCultureIgnoreCase));

                var matchingTitleWithQuality = matchigTitle.Where(q => q.ParseResult.Quality >= newParseResult.Quality);

                if (newParseResult.Series.SeriesTypes == SeriesTypes.Daily)
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

            catch (Exception ex)
            {
                logger.WarnException("Unable to connect to SABnzbd to check queue.", ex);
                return false;
            }
        }

        public virtual bool DownloadNzb(string url, string title, bool recentlyAired)
        {
            try
            {
                string cat = _configService.SabTvCategory;
                int priority = recentlyAired ? (int)_configService.SabRecentTvPriority : (int)_configService.SabBacklogTvPriority;

                string name = url.Replace("&", "%26");
                string nzbName = HttpUtility.UrlEncode(title);

                string action = string.Format("mode=addurl&name={0}&priority={1}&pp=3&cat={2}&nzbname={3}&output=json",
                    name, priority, cat, nzbName);

                string request = GetSabRequest(action);
                logger.Info("Adding report [{0}] to the queue.", title);

                var response = _httpProvider.DownloadString(request);
            
                logger.Debug("Queue Response: [{0}]", response);

                CheckForError(response);
                return true;
            }

            catch (WebException ex)
            {
                logger.Error("Error communicating with SAB: " + ex.Message);
            }

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
                host = _configService.SabHost;

            if (port == 0)
                port = _configService.SabPort;

            if (apiKey == null)
                apiKey = _configService.SabApiKey;

            if (username == null)
                username = _configService.SabUsername;

            if (password == null)
                password = _configService.SabPassword;

            const string action = "mode=get_cats&output=json";

            var command = string.Format(@"http://{0}:{1}/api?{2}&apikey={3}&ma_username={4}&ma_password={5}",
                                 host, port, action, apiKey, username, password);

            var response = _httpProvider.DownloadString(command);

            if (String.IsNullOrWhiteSpace(response))
                return new SabCategoryModel { categories = new List<string>() };

            var categories = JsonConvert.DeserializeObject<SabCategoryModel>(response);

            return categories;
        }

        public virtual SabVersionModel GetVersion(string host = null, int port = 0, string apiKey = null, string username = null, string password = null)
        {
            //Get saved values if any of these are defaults
            if (host == null)
                host = _configService.SabHost;

            if (port == 0)
                port = _configService.SabPort;

            if (apiKey == null)
                apiKey = _configService.SabApiKey;

            if (username == null)
                username = _configService.SabUsername;

            if (password == null)
                password = _configService.SabPassword;

            const string action = "mode=version&output=json";

            var command = string.Format(@"http://{0}:{1}/api?{2}&apikey={3}&ma_username={4}&ma_password={5}",
                                 host, port, action, apiKey, username, password);

            var response = _httpProvider.DownloadString(command);

            if (String.IsNullOrWhiteSpace(response))
                return null;

            var version = JsonConvert.DeserializeObject<SabVersionModel>(response);

            return version;
        }

        public virtual string Test(string host, int port, string apiKey, string username, string password)
        {
            try
            {
                var version = GetVersion(host, port, apiKey, username, password);
                return version.Version;
            }
            catch(Exception ex)
            {
                logger.DebugException("Failed to Test SABnzbd", ex);
            }
            
            return String.Empty;
        }

        private string GetSabRequest(string action)
        {
            return string.Format(@"http://{0}:{1}/api?{2}&apikey={3}&ma_username={4}&ma_password={5}",
                                 _configService.SabHost,
                                 _configService.SabPort,
                                 action,
                                 _configService.SabApiKey,
                                 _configService.SabUsername,
                                 _configService.SabPassword);
        }

        private void CheckForError(string response)
        {
            var result = JsonConvert.DeserializeObject<SabJsonError>(response);

            if (result.Status != null && result.Status.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                throw new ApplicationException(result.Error);
        }
    }
}