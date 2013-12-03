using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.DataAugmentation.Xem
{
    public class XemService : IHandle<SeriesUpdatedEvent>
    {
        private readonly IEpisodeService _episodeService;
        private readonly IXemProxy _xemProxy;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;
        private readonly ICached<bool> _cache;

        public XemService(IEpisodeService episodeService,
                           IXemProxy xemProxy,
                           ISeriesService seriesService, ICacheManger cacheManger, Logger logger)
        {
            _episodeService = episodeService;
            _xemProxy = xemProxy;
            _seriesService = seriesService;
            _logger = logger;
            _logger = logger;
            _cache = cacheManger.GetCache<bool>(GetType());
        }


        public void Handle(SeriesUpdatedEvent message)
        {
            if (_cache.Count == 0)
            {
                RefreshCache();
            }

            if (!_cache.Find(message.Series.TvdbId.ToString()))
            {
                _logger.Trace("Scene numbering is not available for {0} [{1}]", message.Series.Title, message.Series.TvdbId);
                return;
            }

            PerformUpdate(message.Series);
        }

        private void PerformUpdate(Series series)
        {
            _logger.Trace("Updating scene numbering mapping for: {0}", series);

            try
            {
                var mappings = _xemProxy.GetSceneTvdbMappings(series.TvdbId);

                if (!mappings.Any())
                {
                    _logger.Trace("Mappings for: {0} are empty, skipping", series);
                    _cache.Remove(series.TvdbId.ToString());
                    return;
                }

                var episodes = _episodeService.GetEpisodeBySeries(series.Id);

                foreach (var episode in episodes)
                {
                    episode.AbsoluteEpisodeNumber = 0;
                    episode.SceneSeasonNumber = 0;
                    episode.SceneEpisodeNumber = 0;
                }

                foreach (var mapping in mappings)
                {
                    _logger.Trace("Setting scene numbering mappings for {0} S{1:00}E{2:00}", series, mapping.Tvdb.Season, mapping.Tvdb.Episode);

                    var episode = episodes.SingleOrDefault(e => e.SeasonNumber == mapping.Tvdb.Season && e.EpisodeNumber == mapping.Tvdb.Episode);

                    if (episode == null)
                    {
                        _logger.Trace("Information hasn't been added to TheTVDB yet, skipping.");
                        continue;
                    }

                    episode.AbsoluteEpisodeNumber = mapping.Scene.Absolute;
                    episode.SceneSeasonNumber = mapping.Scene.Season;
                    episode.SceneEpisodeNumber = mapping.Scene.Episode;
                }

                _episodeService.UpdateEpisodes(episodes);
                series.UseSceneNumbering = true;
                _seriesService.UpdateSeries(series);

                _logger.Debug("XEM mapping updated for {0}", series);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error updating scene numbering mappings for: " + series, ex);
            }
        }

        private void RefreshCache()
        {
            _cache.Clear();

            var ids = _xemProxy.GetXemSeriesIds();

            foreach (var id in ids)
            {
                _cache.Set(id.ToString(), true, TimeSpan.FromHours(1));
            }
        }
    }
}
