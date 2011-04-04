using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using Rss;

namespace NzbDrone.Core.Providers
{
    public class RssSyncProvider : IRssSyncProvider
    {
        //Sync with RSS feeds to download files as needed

        private Thread _rssSyncThread;
        private IIndexerProvider _indexerProvider;
        private IRssProvider _rss;
        private ISeriesProvider _series;
        private ISeasonProvider _season;
        private IEpisodeProvider _episode;
        private IHistoryProvider _history;
        private IDownloadProvider _sab;
        private IConfigProvider _configProvider;
        private IRssItemProcessingProvider _rssItemProcessor;
        private readonly INotificationProvider _notificationProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ProgressNotification _rssSyncNotification;

        public RssSyncProvider(IIndexerProvider indexerProvider, IRssProvider rss,
            ISeriesProvider series, ISeasonProvider season,
            IEpisodeProvider episode, IHistoryProvider history,
            IDownloadProvider sab, INotificationProvider notificationProvider,
            IConfigProvider configProvider, IRssItemProcessingProvider rssItemProcessor)
        {
            _indexerProvider = indexerProvider;
            _rss = rss;
            _series = series;
            _season = season;
            _episode = episode;
            _history = history;
            _sab = sab;
            _notificationProvider = notificationProvider;
            _configProvider = configProvider;
            _rssItemProcessor = rssItemProcessor;
        }

        #region IRssSyncProvider Members

        public void Begin()
        {
            Logger.Debug("RSS Sync Starting");
            if (_rssSyncThread == null || !_rssSyncThread.IsAlive)
            {
                Logger.Debug("Initializing background sync of RSS Feeds.");
                _rssSyncThread = new Thread(SyncWithRss)
                {
                    Name = "RssSync",
                    Priority = ThreadPriority.Lowest
                };

                _rssSyncThread.Start();
            }
            else
            {
                Logger.Warn("RSS Sync already in progress. Ignoring request.");
            }

        }

        #endregion

        private void SyncWithRss()
        {
            //Get all enabled RSS providers
            //Download Feeds

            var indexers = _indexerProvider.EnabledIndexers();

            using (_rssSyncNotification = new ProgressNotification("RSS Sync"))
            {
                _notificationProvider.Register(_rssSyncNotification);
                _rssSyncNotification.CurrentStatus = "Starting Scan";
                _rssSyncNotification.ProgressMax = indexers.Count();

                foreach (var i in indexers)
                {
                    Logger.Info("Starting RSS Sync for: {0}", i.IndexerName);
                    //Need to insert the users information in the the URL before trying to use it
                    GetUsersUrl(i); //Get the new users specific url (with their information) to use for the Sync

                    //If the url still contains '{' & '}' the user probably hasn't configured the indexer settings
                    if (i.RssUrl.Contains("{") && i.RssUrl.Contains("}"))
                    {
                        Logger.Debug("Unable to Sync {0}. User Information has not been configured.", i.IndexerName);
                        continue; //Skip this indexer
                    }

                    _rssSyncNotification.CurrentStatus = String.Format("Syncing with RSS Feed: {0}", i.IndexerName);

                    var indexer = new FeedInfoModel(i.IndexerName, i.RssUrl);

                    var feedItems = _rss.GetFeed(indexer);

                    if (feedItems.Count() == 0)
                    {
                        _rssSyncNotification.CurrentStatus = String.Format("Failed to download RSS Feed: {0}", //
                                                                           i.IndexerName);
                        continue; //No need to process anything else
                    }

                    foreach (RssItem item in feedItems)
                    {
                        NzbInfoModel nzb = Parser.ParseNzbInfo(indexer, item);
                        _rssItemProcessor.DownloadIfWanted(nzb, i);
                    }
                }
                _rssSyncNotification.CurrentStatus = "RSS Sync Completed";
                Logger.Info("RSS Sync has successfully completed.");
                Thread.Sleep(3000);
                _rssSyncNotification.Status = ProgressNotificationStatus.Completed;
            }
        }

        private void GetUsersUrl(Indexer indexer)
        {
            if (indexer.IndexerName == "NzbMatrix")
            {
                var nzbMatrixUsername = _configProvider.GetValue("NzbMatrixUsername", String.Empty, false);
                var nzbMatrixApiKey = _configProvider.GetValue("NzbMatrixApiKey", String.Empty, false);

                if (!String.IsNullOrEmpty(nzbMatrixUsername) && !String.IsNullOrEmpty(nzbMatrixApiKey))
                    indexer.RssUrl = indexer.RssUrl.Replace("{USERNAME}", nzbMatrixUsername).Replace("{APIKEY}", nzbMatrixApiKey);

                //Todo: Perform validation at the config level so a user is unable to enable a provider until user details are provided
                return;
            }

            if (indexer.IndexerName == "NzbsOrg")
            {
                var nzbsOrgUId = _configProvider.GetValue("NzbsOrgUId", String.Empty, false);
                var nzbsOrgHash = _configProvider.GetValue("NzbsOrgHash", String.Empty, false);

                if (!String.IsNullOrEmpty(nzbsOrgUId) && !String.IsNullOrEmpty(nzbsOrgHash))
                    indexer.RssUrl = indexer.RssUrl.Replace("{UID}", nzbsOrgUId).Replace("{HASH}", nzbsOrgHash);

                //Todo: Perform validation at the config level so a user is unable to enable a provider until user details are provided
                return;
            }

            if (indexer.IndexerName == "NzbsOrg")
            {
                var nzbsrusUId = _configProvider.GetValue("NzbsrusUId", String.Empty, false);
                var nzbsrusHash = _configProvider.GetValue("NzbsrusHash", String.Empty, false);

                if (!String.IsNullOrEmpty(nzbsrusUId) && !String.IsNullOrEmpty(nzbsrusHash))
                    indexer.RssUrl = indexer.RssUrl.Replace("{UID}", nzbsrusUId).Replace("{HASH}", nzbsrusHash);

                //Todo: Perform validation at the config level so a user is unable to enable a provider until user details are provided
                return;
            }

            return; //Currently other providers do not require user information to be substituted, simply return
        }
    }
}
