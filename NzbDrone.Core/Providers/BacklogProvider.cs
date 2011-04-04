using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using Rss;

namespace NzbDrone.Core.Providers
{
    public class BacklogProvider : IBacklogProvider
    {
        private readonly ISeriesProvider _seriesProvider;
        private readonly INotificationProvider _notificationProvider;
        private readonly IConfigProvider _configProvider;
        private readonly IIndexerProvider _indexerProvider;
        private readonly IRssProvider _rssProvider;
        private readonly IRssItemProcessingProvider _rssItemProcessor;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private List<Series> _seriesList;
        private Thread _backlogThread;
        private ProgressNotification _backlogSearchNotification;

        public BacklogProvider(ISeriesProvider seriesProvider, INotificationProvider notificationProvider,
            IConfigProvider configProvider, IIndexerProvider indexerProvider,
            IRssProvider rssProvider, IRssItemProcessingProvider _rssItemProcessor)
        {
            _seriesProvider = seriesProvider;
            _notificationProvider = notificationProvider;
            _configProvider = configProvider;
            _indexerProvider = indexerProvider;
            _rssProvider = rssProvider;

            _seriesList = new List<Series>();
        }

        #region IBacklogProvider Members

        public bool StartSearch()
        {
            Logger.Debug("Backlog Search Requested");
            if (_backlogThread == null || !_backlogThread.IsAlive)
            {
                Logger.Debug("Initializing Backlog Search");
                _backlogThread = new Thread(PerformSearch)
                {
                    Name = "BacklogSearch",
                    Priority = ThreadPriority.Lowest
                };

                _seriesList.AddRange(_seriesProvider.GetAllSeries());
                _backlogThread.Start();
            }
            else
            {
                Logger.Warn("Backlog Search already in progress. Ignoring request.");

                //return false if backlog search was already running
                return false;
            }

            //return true if backlog search has started
            return true;
        }

        public bool StartSearch(int seriesId)
        {
            //Get the series
            //Start new Thread if one isn't already started

            Logger.Debug("Backlog Search Requested");
            if (_backlogThread == null || !_backlogThread.IsAlive)
            {
                Logger.Debug("Initializing Backlog Search");
                _backlogThread = new Thread(PerformSearch)
                {
                    Name = "BacklogSearch",
                    Priority = ThreadPriority.Lowest
                };

                var series = _seriesProvider.GetSeries(seriesId);

                if (series == null)
                {
                    Logger.Debug("Invalid Series - Not starting Backlog Search");
                    return false;
                }

                _seriesList.Add(series);
                _backlogThread.Start();
            }
            else
            {
                Logger.Warn("Backlog Search already in progress. Ignoring request.");

                //return false if backlog search was already running
                return false;
            }

            //return true if backlog search has started
            return true;
        }

        #endregion

        private void PerformSearch()
        {
            try
            {
                using (_backlogSearchNotification = new ProgressNotification("Series Scan"))
                {
                    _notificationProvider.Register(_backlogSearchNotification);
                    _backlogSearchNotification.CurrentStatus = "Starting Backlog Search";
                    _backlogSearchNotification.ProgressMax = _seriesList.Count;

                    foreach (var series in _seriesList)
                    {
                        BackLogSeries(series);
                    }

                    _backlogSearchNotification.CurrentStatus = "Backlog Search Completed";
                    Logger.Info("Backlog Search has successfully completed.");
                    Thread.Sleep(3000);
                    _backlogSearchNotification.Status = ProgressNotificationStatus.Completed;
                }
            }

            catch (Exception ex)
            {
                Logger.WarnException(ex.Message, ex);
            }
        }

        private void BackLogSeries(Series series)
        {
            try
            {
                //Do the searching here
                _backlogSearchNotification.CurrentStatus = String.Format("Backlog Searching For: {0}", series.Title);

                var sceneNames = SceneNameHelper.FindById(series.SeriesId);

                if (sceneNames.Count < 1)
                    sceneNames.Add(series.Title);

                foreach (var season in series.Seasons)
                {
                    BackLogSeason(sceneNames, season);
                }
                //Done searching for each episode
            }

            catch (Exception ex)
            {
                Logger.WarnException(ex.Message, ex);
            }

            _backlogSearchNotification.ProgressValue++;
        }

        private void BackLogSeason(List<string> sceneNames, Season season)
        {
            var episodesWithoutFiles = season.Episodes.Where(e => e.EpisodeFileId == 0);

            if (season.Episodes.Count() == episodesWithoutFiles.Count())
            {
                //Whole season needs to be grabbed, look for the whole season first
                //Lookup scene name using seriesId

                foreach (var sceneName in sceneNames)
                {
                    var searchString = String.Format("{0} Season {1}", sceneName,
                                                     season.SeasonNumber);

                    foreach (var i in _indexerProvider.EnabledIndexers())
                    {
                        //Get the users URL
                        GetUsersUrl(i, searchString);

                        //If the url still contains '{' & '}' the user probably hasn't configured the indexer settings
                        if (i.ApiUrl.Contains("{") && i.ApiUrl.Contains("}"))
                        {
                            Logger.Debug("Unable to Sync {0}. User Information has not been configured.", i.IndexerName);
                            continue; //Skip this indexer
                        }

                        var indexer = new FeedInfoModel(i.IndexerName, i.ApiUrl);

                        var feedItems = _rssProvider.GetFeed(indexer);

                        if (feedItems.Count() == 0)
                        {
                            Logger.Debug("Failed to download Backlog Search URL: {0}", indexer.Name);
                            continue; //No need to process anything else
                        }

                        foreach (RssItem item in feedItems)
                        {
                            NzbInfoModel nzb = Parser.ParseNzbInfo(indexer, item);
                            QueueSeasonIfWanted(nzb, i);
                        }

                    }
                }
            }
        }

        private void GetUsersUrl(Indexer indexer, string searchString)
        {
            if (indexer.IndexerName == "NzbMatrix")
            {
                var nzbMatrixUsername = _configProvider.GetValue("NzbMatrixUsername", String.Empty, false);
                var nzbMatrixApiKey = _configProvider.GetValue("NzbMatrixApiKey", String.Empty, false);
                var retention = Convert.ToInt32(_configProvider.GetValue("Retention", String.Empty, false));

                if (!String.IsNullOrEmpty(nzbMatrixUsername) && !String.IsNullOrEmpty(nzbMatrixApiKey))
                    indexer.ApiUrl = indexer.ApiUrl.Replace("{USERNAME}", nzbMatrixUsername).Replace("{APIKEY}", nzbMatrixApiKey).Replace("{AGE}", retention.ToString()).Replace("{TERM}", searchString);

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

        private void QueueSeasonIfWanted(NzbInfoModel nzb, Indexer indexer)
        {
            //Do we want this item?
            try
            {
                if (nzb.IsPassworded())
                {
                    Logger.Debug("Skipping Passworded Report {0}", nzb.Title);
                    return;
                }

                //Need to get REGEX that will handle "Show Name Season 1 quality"
                nzb.TitleFix = String.Empty;
                nzb.TitleFix = String.Format("{0} [{1}]", nzb.TitleFix, nzb.Quality); //Add Quality to the titleFix

            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
            }
        }
    }
}
