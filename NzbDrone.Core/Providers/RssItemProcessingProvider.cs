using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class RssItemProcessingProvider : IRssItemProcessingProvider
    {
        private ISeriesProvider _seriesProvider;
        private ISeasonProvider _seasonProvider;
        private IEpisodeProvider _episodeProvider;
        private IHistoryProvider _historyProvider;
        private IDownloadProvider _sabProvider;
        private IConfigProvider _configProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RssItemProcessingProvider(ISeriesProvider seriesProvider, ISeasonProvider seasonProvider, 
            IEpisodeProvider episodeProvider, IHistoryProvider historyProvider,
            IDownloadProvider sabProvider, IConfigProvider configProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
            _historyProvider = historyProvider;
            _sabProvider = sabProvider;
            _configProvider = configProvider;
        }

        #region IRssItemProcessingProvider Members

        public void QueueIfWanted(NzbInfoModel nzb, Indexer indexer)
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

                if (episodeParseResults.Count() > 1)
                {
                    ProcessStandardItem(nzb, indexer, episodeParseResults);
                    return;
                }

                //Try to handle Season X style naming

                if (episodeParseResults.Count() < 1)
                {
                    Logger.Debug("Unsupported Title: {0}", nzb.Title);
                    return;
                }
            }

            catch (Exception ex)
            {
                Logger.ErrorException("Error Parsing NZB: " + ex.Message, ex);
            }
        }

        public string GetTitleFix(List<EpisodeParseResult> episodes, int seriesId)
        {
            var series = _seriesProvider.GetSeries(seriesId);

            int seasonNumber = 0;
            string episodeNumbers = String.Empty;
            string episodeTitles = String.Empty;

            foreach (var episode in episodes)
            {
                var episodeInDb = _episodeProvider.GetEpisode(seriesId, episode.SeasonNumber, episode.EpisodeNumber);

                if (episodeInDb == null)
                {
                    Logger.Debug("Episode Not found in Database");
                    return String.Format("{0} - {1:00}x{2}", series.Title, episode.SeasonNumber, episode.SeriesTitle);
                }

                seasonNumber = episodeInDb.SeasonNumber;
                episodeNumbers = String.Format("{0}x{1:00}", episodeNumbers, episodeInDb.EpisodeNumber);
                episodeTitles = String.Format("{0} + {1}", episodeTitles, episodeInDb.Title);
            }

            episodeTitles = episodeTitles.Trim(' ', '+');

            return String.Format("{0} - {1}{2} - {3}", series.Title, seasonNumber, episodeNumbers, episodeTitles);
        }

        #endregion

        private void ProcessStandardItem(NzbInfoModel nzb, Indexer indexer, List<EpisodeParseResult> episodeParseResults)
        {
            //Will try to match via NormalizeTitle, if that fails it will look for a scene name and do a lookup for your shows
            var series = _seriesProvider.FindSeries(episodeParseResults[0].SeriesTitle);

            if (series == null)
            {
                //If we weren't able to find a title using the clean name, lets try again looking for a scene name
                series = _seriesProvider.GetSeries(SceneNameHelper.FindByName(episodeParseResults[0].SeriesTitle));

                if (series == null)
                {
                    Logger.Debug("Show is not being watched: {0}", episodeParseResults[0].SeriesTitle);
                    return;
                }
            }

            Logger.Debug("Show is being watched: {0}", series.Title);

            nzb.TitleFix = GetTitleFix(episodeParseResults, series.SeriesId); //Get the TitleFix so we can use it later

            nzb.Proper = Parser.ParseProper(nzb.Title);
            nzb.Quality = Parser.ParseQuality(nzb.Title);

            nzb.TitleFix = String.Format("{0} [{1}]", nzb.TitleFix, nzb.Quality); //Add Quality to the titleFix

            //Loop through the list of the episodeParseResults to ensure that all the episodes are needed
            foreach (var episode in episodeParseResults)
            {
                //IsEpisodeWanted?
                var episodeModel = new EpisodeModel();
                episodeModel.Proper = nzb.Proper;
                episodeModel.SeriesId = series.SeriesId;
                episodeModel.SeriesTitle = series.Title;
                episodeModel.Quality = nzb.Quality;
                episodeModel.SeasonNumber = episode.SeasonNumber;
                episodeModel.EpisodeNumber = episode.EpisodeNumber;

                if (!_episodeProvider.IsNeeded(episodeModel))
                    return;

                var titleFix = GetTitleFix(new List<EpisodeParseResult> { episode }, episodeModel.SeriesId);
                titleFix = String.Format("{0} [{1}]", titleFix, nzb.Quality); //Add Quality to the titleFix

                if (_sabProvider.IsInQueue(titleFix))
                    return;
            }

            //If their is more than one episode in this NZB check to see if it has been added as a single NZB
            if (episodeParseResults.Count > 1)
            {
                if (_sabProvider.IsInQueue(nzb.TitleFix))
                    return;
            }

            //Only add to history if it was added to properly sent to SABnzbd
            if (indexer.IndexerName != "Newzbin")
                AddByUrl(episodeParseResults, series, nzb, indexer);

            else
            {
                //AddById(episodeParseResults, series, nzb, indexer);
            }
        }

        private void AddByUrl(List<EpisodeParseResult> episodeParseResults, Series series, NzbInfoModel nzb, Indexer indexer)
        {
            if (_sabProvider.AddByUrl(nzb.Link.ToString(), nzb.TitleFix))
            {
                //We need to loop through the episodeParseResults so each episode in the NZB is properly handled
                foreach (var epr in episodeParseResults)
                {
                    var episode = _episodeProvider.GetEpisode(series.SeriesId, epr.SeasonNumber, epr.EpisodeNumber);

                    if (episode == null)
                    {
                        //Not sure how we got this far, so lets throw an exception
                        throw new ArgumentOutOfRangeException();
                    }

                    //Set episode status to grabbed
                    episode.Status = EpisodeStatusType.Grabbed;

                    //Add to History
                    var history = new History();
                    history.Date = DateTime.Now;
                    history.EpisodeId = episode.EpisodeId;
                    history.IndexerName = indexer.IndexerName;
                    history.IsProper = nzb.Proper;
                    history.Quality = nzb.Quality;
                    history.NzbTitle = nzb.Title;

                    _historyProvider.Insert(history);
                }
            }
        }
    }
}
