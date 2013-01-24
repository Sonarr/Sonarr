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
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Nzbget;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.DownloadClients
{
    public class NzbgetProvider : IDownloadClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly HttpProvider _httpProvider;

        public NzbgetProvider(ConfigProvider configProvider, HttpProvider httpProvider)
        {
            _configProvider = configProvider;
            _httpProvider = httpProvider;
        }

        public NzbgetProvider()
        {
        }

        public virtual bool IsInQueue(EpisodeParseResult newParseResult)
        {
            try
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

            catch (Exception ex)
            {
                logger.WarnException("Unable to connect to Nzbget to check queue.", ex);
                return false;
            }
        }

        public virtual bool DownloadNzb(string url, string title, bool recentlyAired)
        {
            try
            {
                string cat = _configProvider.NzbgetTvCategory;
                int priority = recentlyAired ? (int)_configProvider.NzbgetRecentTvPriority : (int)_configProvider.NzbgetBacklogTvPriority;

                var command = new JsonRequest
                {
                    Method = "appendurl",
                    Params = new object[]{ title, cat, priority, false, url }
                };

                logger.Info("Adding report [{0}] to the queue.", title);
                var response = PostCommand(JsonConvert.SerializeObject(command));

                CheckForError(response);

                var success = JsonConvert.DeserializeObject<EnqueueResponse>(response).Result;
                logger.Debug("Queue Response: [{0}]", success);

                return true;
            }

            catch (WebException ex)
            {
                logger.Error("Error communicating with Nzbget: " + ex.Message);
            }

            return false;
        }

        public virtual List<QueueItem> GetQueue()
        {
            var command = new JsonRequest
                {
                        Method = "listgroups",
                        Params = null
                };

            var response = PostCommand(JsonConvert.SerializeObject(command));

            CheckForError(response);

            return JsonConvert.DeserializeObject<Queue>(response).QueueItems;
        }

        public virtual VersionModel GetVersion(string host = null, int port = 0, string username = null, string password = null)
        {
            //Get saved values if any of these are defaults
            if (host == null)
                host = _configProvider.NzbgetHost;

            if (port == 0)
                port = _configProvider.NzbgetPort;

            if (username == null)
                username = _configProvider.NzbgetUsername;

            if (password == null)
                password = _configProvider.NzbgetPassword;

            var command = new JsonRequest
            {
                Method = "version",
                Params = null
            };

            var url = String.Format(@"http://{0}:{1}/jsonrpc", host, port);
            var response = _httpProvider.PostCommand(url, username, password, JsonConvert.SerializeObject(command));

            CheckForError(response);

            return JsonConvert.DeserializeObject<VersionModel>(response);
        }

        public virtual string Test(string host, int port, string apiKey, string username, string password)
        {
            try
            {
                var version = GetVersion(host, port, username, password);
                return version.Result;
            }
            catch(Exception ex)
            {
                logger.DebugException("Failed to Test Nzbget", ex);
            }
            
            return String.Empty;
        }

        private string PostCommand(string command)
        {
            var url = String.Format(@"http://{0}:{1}/jsonrpc",
                                 _configProvider.NzbgetHost,
                                 _configProvider.NzbgetPort);

            return _httpProvider.PostCommand(url, _configProvider.NzbgetUsername, _configProvider.NzbgetPassword, command);
        }

        private void CheckForError(string response)
        {
            var result = JsonConvert.DeserializeObject<JsonError>(response);

            if (result.Error != null)
                throw new ApplicationException(result.Error.ToString());
        }
    }
}