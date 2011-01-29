using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using NzbDrone.Core.Model;
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

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RssSyncProvider(IIndexerProvider indexerProvider, IRssProvider rss, ISeriesProvider series,
            ISeasonProvider season, IEpisodeProvider episode, IHistoryProvider history, IDownloadProvider sab)
        {
            _indexerProvider = indexerProvider;
            _rss = rss;
            _series = series;
            _season = season;
            _episode = episode;
            _history = history;
            _sab = sab;
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
                    Name = "SyncUnmappedFolders",
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

            foreach (var i in indexers)
            {
                var indexer = new FeedInfoModel(i.IndexerName, i.RssUrl);

                foreach(RssItem item in  _rss.GetFeed(indexer))
                {
                    NzbInfoModel nzb = Parser.ParseNzbInfo(indexer, item);
                    QueueIfWanted(nzb);
                }
            }
        }

        private void QueueIfWanted(NzbInfoModel nzb)
        {
            //Do we want this item?  

            try
            {
                if (nzb.IsPassworded())
                {
                    Logger.Debug("Skipping Passworded Report {0}", nzb.Title);
                    return;
                }

                var episodeParseResults = Parser.ParseEpisodeInfo(nzb.Title);

                if (episodeParseResults.Count() < 1)
                {
                    Logger.Debug("Unsupported Title: {0}", nzb.Title);
                    return;
                }

                if (episodeParseResults.Count() > 0)
                {
                    //Todo: How to determine if we want the show if the FeedTitle is drastically different from the TitleOnDisk (CSI is one that comes to mind)
                    var series = _series.FindSeries(episodeParseResults[0].SeriesTitle);

                    if (series == null)
                    {
                        Logger.Debug("Show is not being watched: {0}", episodeParseResults[0].SeriesTitle);
                        return;
                    }

                    Logger.Debug("Show is being watched: {0}", series.Title);

                    nzb.TitleFix = GetTitleFix(episodeParseResults, series.SeriesId); //Get the TitleFix so we can use it later
                    nzb.Proper = Parser.ParseProper(nzb.Title);

                    //Loop through the list of the episodeParseResults to ensure that all the episodes are needed)
                    foreach (var episode in episodeParseResults)
                    {
                        //IsEpisodeWanted?
                        var episodeModel = new EpisodeModel();
                        episodeModel.Proper = nzb.Proper;
                        episodeModel.SeriesId = series.SeriesId;
                        episodeModel.SeriesTitle = series.Title;
                        episodeModel.Quality = Parser.ParseQuality(nzb.Title);
                        episodeModel.SeasonNumber = episode.SeasonNumber;
                        episodeModel.EpisodeNumber = episode.EpisodeNumber;

                        if (!_episode.IsNeeded(episodeModel))
                            return;

                        var titleFix = GetTitleFix(new List<EpisodeParseResult> { episode }, episodeModel.SeriesId);
                        _sab.IsInQueue(titleFix);
                    }
                }
            }

            catch (Exception ex)
            {
                Logger.ErrorException("Error Parsing NZB: " + ex.Message, ex);
            }
        }

        private string GetTitleFix(List<EpisodeParseResult> episodes, int seriesId)
        {
            var series = _series.GetSeries(seriesId);

            int seasonNumber = 0;
            string episodeNumbers = String.Empty;
            string episodeTitles = String.Empty;

            foreach (var episode in episodes)
            {
                var episodeInDb = _episode.GetEpisode(seriesId, episode.SeasonNumber, episode.EpisodeNumber);
                seasonNumber = episodeInDb.SeasonNumber;
                episodeNumbers = String.Format("{0}x{1:00}", episodeNumbers, episodeInDb.EpisodeNumber);
                episodeTitles = String.Format("{0} + {1}", episodeTitles, episodeInDb.Title);
            }

            return String.Format("{0} - {1}{2} - {3}", series.Title, seasonNumber, episodeNumbers, episodeTitles);
        }
    }
}
