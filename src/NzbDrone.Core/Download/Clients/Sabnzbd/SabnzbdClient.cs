using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabRequestBuilder
    {
        private readonly IConfigService _configService;

        public SabRequestBuilder(IConfigService configService)
        {
            _configService = configService;
        }

        public IRestRequest AddToQueueRequest(RemoteEpisode remoteEpisode)
        {
            string cat = _configService.SabTvCategory;
            int priority = (int)_configService.SabRecentTvPriority;

            string name = remoteEpisode.Release.DownloadUrl.Replace("&", "%26");
            string nzbName = HttpUtility.UrlEncode(remoteEpisode.Release.Title);

            string action = string.Format("mode=addurl&name={0}&priority={1}&pp=3&cat={2}&nzbname={3}&output=json",
                name, priority, cat, nzbName);

            string request = GetSabRequest(action);

            return new RestRequest(request);
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
    }

    public class SabnzbdClient : IDownloadClient
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;
        private readonly IParsingService _parsingService;
        private readonly ISabCommunicationProxy _sabCommunicationProxy;
        private readonly ICached<IEnumerable<QueueItem>> _queueCache;
        private readonly Logger _logger;

        public SabnzbdClient(IConfigService configService,
                             IHttpProvider httpProvider,
                             ICacheManger cacheManger,
                             IParsingService parsingService,
                             ISabCommunicationProxy sabCommunicationProxy,
                             Logger logger)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _parsingService = parsingService;
            _sabCommunicationProxy = sabCommunicationProxy;
            _queueCache = cacheManger.GetCache<IEnumerable<QueueItem>>(GetType(), "queue");
            _logger = logger;
        }

        public void DownloadNzb(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title;
            var category = _configService.SabTvCategory;
            var priority = remoteEpisode.IsRecentEpisode() ? (int)_configService.SabRecentTvPriority : (int)_configService.SabOlderTvPriority;

            using (var nzb = _httpProvider.DownloadStream(url))
            {
                _logger.Info("Adding report [{0}] to the queue.", title);
                var response = _sabCommunicationProxy.DownloadNzb(nzb, title, category, priority);

                _logger.Debug("Queue Response: [{0}]", response);
            }
        }

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_configService.SabHost)
                    && _configService.SabPort != 0;
            }
        }

        public IEnumerable<QueueItem> GetQueue()
        {
            return _queueCache.Get("queue", () =>
            {
                string action = String.Format("mode=queue&output=json&start={0}&limit={1}", 0, 0);
                string request = GetSabRequest(action);
                string response = _httpProvider.DownloadString(request);

                CheckForError(response);

                var sabQueue = Json.Deserialize<SabQueue>(JObject.Parse(response).SelectToken("queue").ToString()).Items;

                var queueItems = new List<QueueItem>();

                foreach (var sabQueueItem in sabQueue)
                {
                    var queueItem = new QueueItem();
                    queueItem.Id = sabQueueItem.Id;
                    queueItem.Title = sabQueueItem.Title;
                    queueItem.Size = sabQueueItem.Size;
                    queueItem.Sizeleft = sabQueueItem.Sizeleft;
                    queueItem.Timeleft = sabQueueItem.Timeleft;
                    queueItem.Status = sabQueueItem.Status;

                    var parsedEpisodeInfo = Parser.Parser.ParseTitle(queueItem.Title);
                    if (parsedEpisodeInfo == null) continue;

                    var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0);
                    if (remoteEpisode.Series == null) continue;

                    queueItem.RemoteEpisode = remoteEpisode;

                    queueItems.Add(queueItem);
                }

                return queueItems;
            }, TimeSpan.FromSeconds(10));
        }

        public virtual List<SabHistoryItem> GetHistory(int start = 0, int limit = 0)
        {
            string action = String.Format("mode=history&output=json&start={0}&limit={1}", start, limit);
            string request = GetSabRequest(action);
            string response = _httpProvider.DownloadString(request);

            CheckForError(response);

            var items = Json.Deserialize<SabHistory>(JObject.Parse(response).SelectToken("history").ToString()).Items;
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

            var categories = Json.Deserialize<SabCategoryModel>(response);

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

            var version = Json.Deserialize<SabVersionModel>(response);

            return version;
        }

        public virtual string Test(string host, int port, string apiKey, string username, string password)
        {
            try
            {
                var version = GetVersion(host, port, apiKey, username, password);
                return version.Version;
            }
            catch (Exception ex)
            {
                _logger.DebugException("Failed to Test SABnzbd", ex);
            }

            return String.Empty;
        }

        private string GetSabRequest(string action)
        {
            var protocol = _configService.SabUseSsl ? "https" : "http";

            return string.Format(@"{0}://{1}:{2}/api?{3}&apikey={4}&ma_username={5}&ma_password={6}",
                                 protocol,
                                 _configService.SabHost,
                                 _configService.SabPort,
                                 action,
                                 _configService.SabApiKey,
                                 _configService.SabUsername,
                                 _configService.SabPassword);
        }

        private void CheckForError(string response)
        {
            var result = Json.Deserialize<SabJsonError>(response);

            if (result.Status != null && result.Status.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                throw new ApplicationException(result.Error);
        }
    }
}