using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetClient : IDownloadClient
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public NzbgetClient(IConfigService configService, IHttpProvider httpProvider, Logger logger)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public virtual bool DownloadNzb(string url, string title)
        {
            try
            {
                string cat = _configService.NzbgetTvCategory;
                int priority = (int)_configService.NzbgetRecentTvPriority;

                var command = new JsonRequest
                {
                    Method = "appendurl",
                    Params = new object[] { title, cat, priority, false, url }
                };

                _logger.Info("Adding report [{0}] to the queue.", title);
                var response = PostCommand(JsonConvert.SerializeObject(command));

                CheckForError(response);

                var success = JsonConvert.DeserializeObject<EnqueueResponse>(response).Result;
                _logger.Debug("Queue Response: [{0}]", success);

                return true;
            }

            catch (WebException ex)
            {
                _logger.Error("Error communicating with Nzbget: " + ex.Message);
            }

            return false;
        }

        public virtual IEnumerable<QueueItem> GetQueue()
        {
            var command = new JsonRequest
                {
                    Method = "listgroups",
                    Params = null
                };

            var response = PostCommand(JsonConvert.SerializeObject(command));

            CheckForError(response);

            var itmes = JsonConvert.DeserializeObject<NzbGetQueue>(response).QueueItems;

            foreach (var nzbGetQueueItem in itmes)
            {
                var queueItem = new QueueItem();
                queueItem.Id = nzbGetQueueItem.NzbId.ToString();
                queueItem.Title = nzbGetQueueItem.NzbName;
                queueItem.Size = nzbGetQueueItem.FileSizeMb;
                queueItem.SizeLeft = nzbGetQueueItem.RemainingSizeMb;

                yield return queueItem;
            }
        }

        public virtual VersionModel GetVersion(string host = null, int port = 0, string username = null, string password = null)
        {
            //Get saved values if any of these are defaults
            if (host == null)
                host = _configService.NzbgetHost;

            if (port == 0)
                port = _configService.NzbgetPort;

            if (username == null)
                username = _configService.NzbgetUsername;

            if (password == null)
                password = _configService.NzbgetPassword;

            var command = new JsonRequest
            {
                Method = "version",
                Params = null
            };

            var address = String.Format(@"{0}:{1}", host, port);
            var response = _httpProvider.PostCommand(address, username, password, JsonConvert.SerializeObject(command));

            CheckForError(response);

            return JsonConvert.DeserializeObject<VersionModel>(response);
        }

        public virtual string Test(string host, int port, string username, string password)
        {
            try
            {
                var version = GetVersion(host, port, username, password);
                return version.Result;
            }
            catch (Exception ex)
            {
                _logger.DebugException("Failed to Test Nzbget", ex);
            }

            return String.Empty;
        }

        private string PostCommand(string command)
        {
            var url = String.Format(@"{0}:{1}",
                                 _configService.NzbgetHost,
                                 _configService.NzbgetPort);

            return _httpProvider.PostCommand(url, _configService.NzbgetUsername, _configService.NzbgetPassword, command);
        }

        private void CheckForError(string response)
        {
            var result = JsonConvert.DeserializeObject<JsonError>(response);

            if (result.Error != null)
                throw new ApplicationException(result.Error.ToString());
        }
    }
}