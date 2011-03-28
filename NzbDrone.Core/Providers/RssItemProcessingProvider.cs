using System;
using System.Collections.Generic;
using System.IO;
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
        private IHttpProvider _httpProvider;
        private IDiskProvider _diskProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RssItemProcessingProvider(ISeriesProvider seriesProvider, ISeasonProvider seasonProvider, 
            IEpisodeProvider episodeProvider, IHistoryProvider historyProvider,
            IDownloadProvider sabProvider, IConfigProvider configProvider,
            IHttpProvider httpProvider, IDiskProvider diskProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
            _historyProvider = historyProvider;
            _sabProvider = sabProvider;
            _configProvider = configProvider;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
        }

        #region IRssItemProcessingProvider Members

        public void DownloadIfWanted(NzbInfoModel nzb, Indexer indexer)
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

                if (episodeParseResults.Count() > 0)
                {
                    ProcessStandardItem(nzb, indexer, episodeParseResults);
                    return;
                }

                //Handles Full Season NZBs
                var seasonParseResult = Parser.ParseSeasonInfo(nzb.Title);

                if (seasonParseResult != null)
                {
                    ProcessFullSeasonItem(nzb, indexer, seasonParseResult);
                    return;
                }

                Logger.Debug("Unsupported Title: {0}", nzb.Title);
            }

            catch (Exception ex)
            {
                Logger.Error("Unsupported Title: {0}", nzb.Title);
                Logger.ErrorException("Error Parsing/Processing NZB: " + ex.Message, ex);
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
                    //Todo: Handle this some other way?
                    Logger.Debug("Episode Not found in Database, Fake it...");
                    //return String.Format("{0} - {1:00}x{2}", series.Title, episode.SeasonNumber, episode.EpisodeNumber);
                    episodeInDb = new Episode { EpisodeNumber = episode.EpisodeNumber, Title = "TBA" };
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

                var sceneId = SceneNameHelper.FindByName(episodeParseResults[0].SeriesTitle);

                if (sceneId != 0)
                    series = _seriesProvider.GetSeries(sceneId);

                if (series == null)
                {
                    Logger.Debug("Show is not being watched: {0}", episodeParseResults[0].SeriesTitle);
                    return;
                }
            }

            Logger.Debug("Show is being watched: {0}", series.Title);

            nzb.Proper = Parser.ParseProper(nzb.Title);
            nzb.Quality = Parser.ParseQuality(nzb.Title);

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

                //If it is a PROPER we want to put PROPER in the titleFix
                if (nzb.Proper)
                    titleFix = String.Format("{0} [PROPER]", titleFix);

                if (!Convert.ToBoolean(_configProvider.GetValue("UseBlackhole", true, true)))
                    if (_sabProvider.IsInQueue(titleFix))
                        return;
            }

            nzb.TitleFix = GetTitleFix(episodeParseResults, series.SeriesId); //Get the TitleFix so we can use it later
            nzb.TitleFix = String.Format("{0} [{1}]", nzb.TitleFix, nzb.Quality); //Add Quality to the titleFix

            //If it is a PROPER we want to put PROPER in the titleFix
            if (nzb.Proper)
                nzb.TitleFix = String.Format("{0} [PROPER]", nzb.TitleFix);

            if (Convert.ToBoolean(_configProvider.GetValue("UseBlackHole", true, true)))
            {
                if (DownloadNzb(nzb))
                    AddToHistory(episodeParseResults, series, nzb, indexer);
            }
            
            //Send it to SABnzbd
            else
            {
                //Only need to do this check if it contains more than one episode (because we already checked individually before)
                if (episodeParseResults.Count > 1)
                {
                    if (_sabProvider.IsInQueue(nzb.TitleFix))
                        return;
                }

                if (indexer.IndexerName != "Newzbin")
                {
                    if (_sabProvider.AddByUrl(nzb.Link.ToString(), nzb.TitleFix))
                        AddToHistory(episodeParseResults, series, nzb, indexer);
                }
                
                else
                {
                    if (_sabProvider.AddById(nzb.Id, nzb.TitleFix))
                        AddToHistory(episodeParseResults, series, nzb, indexer);
                }
            }
        }

        private void ProcessFullSeasonItem(NzbInfoModel nzb, Indexer indexer, SeasonParseResult seasonParseResult)
        {
            //Will try to match via NormalizeTitle, if that fails it will look for a scene name and do a lookup for your shows
            var series = _seriesProvider.FindSeries(seasonParseResult.SeriesTitle);

            if (series == null)
            {
                //If we weren't able to find a title using the clean name, lets try again looking for a scene name

                var sceneId = SceneNameHelper.FindByName(seasonParseResult.SeriesTitle);

                if (sceneId != 0)
                    series = _seriesProvider.GetSeries(sceneId);

                if (series == null)
                {
                    Logger.Debug("Show is not being watched: {0}", seasonParseResult.SeriesTitle);
                    return;
                }
            }

            Logger.Debug("Show is being watched: {0}", series.Title);

            nzb.Proper = Parser.ParseProper(nzb.Title);
            nzb.Quality = Parser.ParseQuality(nzb.Title);

            if (!_seriesProvider.QualityWanted(series.SeriesId, nzb.Quality))
            {
                Logger.Info("Quality [{0}] is not wanted for: {1}", nzb.Quality, series.Title);
                return;
            }

            var season = _seasonProvider.GetSeason(series.SeriesId, seasonParseResult.SeasonNumber);

            if (season == null)
                return;

            if (_seasonProvider.IsIgnored(season.SeriesId))
                return;

            //Check to see if this is an upgrade for all our files

            var episodesWithoutFiles = season.Episodes.Where(e => e.EpisodeFileId == 0);

            var downloadWholeSeason = false;

            if (season.Episodes.Count() == episodesWithoutFiles.Count())
            {
                //We don't have any episodes for this season, so as it stands right now we need the entire NZB
                //Download!
                downloadWholeSeason = true;
            }

            else
            {
                var episodesNeeded = season.Episodes.Count;

                foreach (var episode in season.Episodes)
                {
                    var episodeModel = new EpisodeModel();
                    episodeModel.Proper = nzb.Proper;
                    episodeModel.SeriesId = series.SeriesId;
                    episodeModel.SeriesTitle = series.Title;
                    episodeModel.Quality = nzb.Quality;
                    episodeModel.SeasonNumber = episode.SeasonNumber;
                    episodeModel.EpisodeNumber = episode.EpisodeNumber;

                    if (!_episodeProvider.IsNeeded(episodeModel))
                    {
                        downloadWholeSeason = false;
                        episodesNeeded--; //Decrement the number of downloads we need, used if we want to replace all existing episodes if this will upgrade over X% of files
                        break; //We only want to download this NZB if ALL episodes can be upgraded by this Season NZB
                    }
                    downloadWholeSeason = true;
                }
            }

            if (downloadWholeSeason)
            {
                //Do the final check to ensure we should download this NZB

                if (Convert.ToBoolean(_configProvider.GetValue("UseBlackHole", true, true)))
                {
                    if (DownloadNzb(nzb))
                    {
                        var episodeParseResults = GetEpisodeParseList(season.Episodes);
                        AddToHistory(episodeParseResults, series, nzb, indexer);
                    }
                }

              //Send it to SABnzbd
                else
                {
                    if (_sabProvider.IsInQueue(nzb.TitleFix))
                        return;

                    if (indexer.IndexerName != "Newzbin")
                    {
                        if (_sabProvider.AddByUrl(nzb.Link.ToString(), nzb.TitleFix))
                        {
                            var episodeParseResults = GetEpisodeParseList(season.Episodes);
                            AddToHistory(episodeParseResults, series, nzb, indexer);
                        }
                            
                    }

                    else
                    {
                        if (_sabProvider.AddById(nzb.Id, nzb.TitleFix))
                        {
                            var episodeParseResults = GetEpisodeParseList(season.Episodes);
                            AddToHistory(episodeParseResults, series, nzb, indexer);
                        }
                    }
                }
            }

            //Possibly grab the whole season if a certain % of the season is missing, rather than for 1 or 2 episodes    
        }

        private List<EpisodeParseResult> GetEpisodeParseList(List<Episode> episodes)
        {
            var episodeParseResults = new List<EpisodeParseResult>();
            episodeParseResults.AddRange(
                episodes.Select(
                    e =>
                    new EpisodeParseResult { EpisodeNumber = e.EpisodeNumber, SeasonNumber = e.SeasonNumber }));

            return episodeParseResults;
        }

        private void AddToHistory(List<EpisodeParseResult> episodeParseResults, Series series, NzbInfoModel nzb, Indexer indexer)
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
                history.IndexerId = indexer.IndexerId;
                history.IsProper = nzb.Proper;
                history.Quality = nzb.Quality;
                history.NzbTitle = nzb.Title;

                _historyProvider.Insert(history);
            }
        }

        private bool DownloadNzb(NzbInfoModel nzb)
        {
            var path = _configProvider.GetValue("BlackholeDirectory", String.Empty, true);

            if (String.IsNullOrEmpty(path))
            {
                //Use the NZBDrone root Directory + /NZBs
                path = CentralDispatch.StartupPath + "NZBs";
                //path = @"C:\Test\NZBs";
            }

            if (_diskProvider.FolderExists(path))
            {
                var filename = path + Path.DirectorySeparatorChar + nzb.TitleFix + ".nzb";

                if (_httpProvider.DownloadFile(nzb.Link.ToString(), filename))
                    return true; 
            }

            Logger.Error("Blackhole Directory doesn't exist, not saving NZB: '{0}'", path);
            return false;
        }
    }
}
