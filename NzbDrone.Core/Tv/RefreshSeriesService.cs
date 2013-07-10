using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Common;

namespace NzbDrone.Core.Tv
{
    public class RefreshSeriesService : IExecute<RefreshSeriesCommand>, IHandleAsync<SeriesAddedEvent>
    {
        private readonly IProvideSeriesInfo _seriesInfo;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly ISeasonRepository _seasonRepository;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;

        public RefreshSeriesService(IProvideSeriesInfo seriesInfo, ISeriesService seriesService, IEpisodeService episodeService,
            ISeasonRepository seasonRepository, IMessageAggregator messageAggregator, Logger logger)
        {
            _seriesInfo = seriesInfo;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _seasonRepository = seasonRepository;
            _messageAggregator = messageAggregator;
            _logger = logger;
        }


        public void Execute(RefreshSeriesCommand message)
        {
            if (message.SeriesId.HasValue)
            {
                var series = _seriesService.GetSeries(message.SeriesId.Value);
                RefreshSeriesInfo(series);
            }
            else
            {
                var allSeries = _seriesService.GetAllSeries().OrderBy(c => c.LastInfoSync).ToList();

                foreach (var series in allSeries)
                {
                    try
                    {
                        RefreshSeriesInfo(series);
                    }
                    catch (Exception e)
                    {
                        _logger.ErrorException("Couldn't refresh info for {0}".Inject(series), e);
                    }
                }
            }
        }

        public void HandleAsync(SeriesAddedEvent message)
        {
            RefreshSeriesInfo(message.Series);
        }

        private void RefreshSeriesInfo(Series series)
        {
            var tuple = _seriesInfo.GetSeriesInfo(series.TvdbId);

            var seriesInfo = tuple.Item1;

            series.Title = seriesInfo.Title;
            series.AirTime = seriesInfo.AirTime;
            series.Overview = seriesInfo.Overview;
            series.Status = seriesInfo.Status;
            series.CleanTitle = Parser.Parser.CleanSeriesTitle(seriesInfo.Title);
            series.LastInfoSync = DateTime.UtcNow;
            series.Runtime = seriesInfo.Runtime;
            series.Images = seriesInfo.Images;
            series.Network = seriesInfo.Network;
            series.FirstAired = seriesInfo.FirstAired;
            _seriesService.UpdateSeries(series);

            RefreshEpisodeInfo(series, tuple.Item2);

            _messageAggregator.PublishEvent(new SeriesUpdatedEvent(series));
        }

        private void RefreshEpisodeInfo(Series series, IEnumerable<Episode> remoteEpisodes)
        {
            _logger.Info("Starting series info refresh for: {0}", series);
            var successCount = 0;
            var failCount = 0;


            var seriesEpisodes = _episodeService.GetEpisodeBySeries(series.Id);

            var seasons = _seasonRepository.GetSeasonBySeries(series.Id);

            var updateList = new List<Episode>();
            var newList = new List<Episode>();

            foreach (var episode in remoteEpisodes.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber))
            {
                try
                {
                    var episodeToUpdate = seriesEpisodes.SingleOrDefault(e => e.TvDbEpisodeId == episode.TvDbEpisodeId);

                    if (episodeToUpdate == null)
                    {
                        episodeToUpdate = seriesEpisodes.SingleOrDefault(e => e.SeasonNumber == episode.SeasonNumber && e.EpisodeNumber == episode.EpisodeNumber);
                    }
                    if (episodeToUpdate == null)
                    {
                        episodeToUpdate = new Episode();
                        newList.Add(episodeToUpdate);

                        //If it is Episode Zero Ignore it (specials, sneak peeks.)
                        if (episode.EpisodeNumber == 0 && episode.SeasonNumber != 1)
                        {
                            episodeToUpdate.Monitored = false;
                        }
                        else
                        {
                            var season = seasons.FirstOrDefault(c => c.SeasonNumber == episode.SeasonNumber);

                            episodeToUpdate.Monitored = season != null ? season.Monitored : true;
                        }
                    }
                    else
                    {
                        updateList.Add(episodeToUpdate);
                    }

                    if ((episodeToUpdate.EpisodeNumber != episode.EpisodeNumber ||
                         episodeToUpdate.SeasonNumber != episode.SeasonNumber) &&
                        episodeToUpdate.EpisodeFileId > 0)
                    {
                        _logger.Debug("Un-linking episode file because the episode number has changed");
                        episodeToUpdate.EpisodeFileId = 0;
                    }

                    episodeToUpdate.SeriesId = series.Id;
                    episodeToUpdate.TvDbEpisodeId = episode.TvDbEpisodeId;
                    episodeToUpdate.EpisodeNumber = episode.EpisodeNumber;
                    episodeToUpdate.SeasonNumber = episode.SeasonNumber;
                    episodeToUpdate.Title = episode.Title;
                    episodeToUpdate.Overview = episode.Overview;
                    episodeToUpdate.AirDate = episode.AirDate;

                    if (episodeToUpdate.AirDate < series.FirstAired) episodeToUpdate.AirDate = null;

                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.FatalException(String.Format("An error has occurred while updating episode info for series {0}", series), e);
                    failCount++;
                }
            }

            var allEpisodes = new List<Episode>();
            allEpisodes.AddRange(newList);
            allEpisodes.AddRange(updateList);

            var groups = allEpisodes.Where(c=>c.AirDate.HasValue).GroupBy(e => new { e.SeriesId, e.AirDate }).Where(g => g.Count() > 1).ToList();

            foreach (var group in groups)
            {
                int episodeCount = 0;
                foreach (var episode in group.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber))
                {
                    episode.AirDate = episode.AirDate.Value.AddMinutes(series.Runtime * episodeCount);
                    episodeCount++;
                }
            }

            _episodeService.UpdateMany(updateList);
            _episodeService.InsertMany(newList);

            if (newList.Any())
            {
                _messageAggregator.PublishEvent(new EpisodeInfoAddedEvent(newList, series));
            }

            if (updateList.Any())
            {
                _messageAggregator.PublishEvent(new EpisodeInfoUpdatedEvent(updateList));
            }

            if (failCount != 0)
            {
                _logger.Info("Finished episode refresh for series: {0}. Successful: {1} - Failed: {2} ",
                            series.Title, successCount, failCount);
            }
            else
            {
                _logger.Info("Finished episode refresh for series: {0}.", series);
            }

            //DeleteEpisodesNotAvailableAnymore(series, remoteEpisodes);
        }


        /*        private void DeleteEpisodesNotAvailableAnymore(Series series, IEnumerable<Episode> onlineEpisodes)
                {
                    //Todo: This will not work as currently implemented - what are we trying to do here?
                    return;
                    _logger.Trace("Starting deletion of episodes that no longer exist in TVDB: {0}", series.Title.WithDefault(series.Id));
                    foreach (var episode in onlineEpisodes)
                    {
                        _episodeRepository.Delete(episode.Id);
                    }

                    _logger.Trace("Deleted episodes that no longer exist in TVDB for {0}", series.Id);
                }*/
    }
}