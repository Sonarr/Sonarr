using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.DataAugmentation.Xem
{
    public class XemService : ISceneMappingProvider, IHandle<SeriesUpdatedEvent>, IHandle<SeriesRefreshStartingEvent>
    {
        private readonly IEpisodeService _episodeService;
        private readonly IXemProxy _xemProxy;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;
        private readonly ICached<bool> _cache;

        public XemService(IEpisodeService episodeService,
                           IXemProxy xemProxy,
                           ISeriesService seriesService, ICacheManager cacheManager, Logger logger)
        {
            _episodeService = episodeService;
            _xemProxy = xemProxy;
            _seriesService = seriesService;
            _logger = logger;
            _logger = logger;
            _cache = cacheManager.GetCache<bool>(GetType());
        }

        private void PerformUpdate(Series series)
        {
            _logger.Debug("Updating scene numbering mapping for: {0}", series);

            try
            {
                var mappings = _xemProxy.GetSceneTvdbMappings(series.TvdbId);

                if (!mappings.Any())
                {
                    _logger.Debug("Mappings for: {0} are empty, skipping", series);
                    _cache.Remove(series.TvdbId.ToString());
                    return;
                }

                var episodes = _episodeService.GetEpisodeBySeries(series.Id);

                foreach (var episode in episodes)
                {
                    episode.SceneAbsoluteEpisodeNumber = null;
                    episode.SceneSeasonNumber = null;
                    episode.SceneEpisodeNumber = null;
                }

                foreach (var mapping in mappings)
                {
                    _logger.Debug("Setting scene numbering mappings for {0} S{1:00}E{2:00}", series, mapping.Tvdb.Season, mapping.Tvdb.Episode);

                    var episode = episodes.SingleOrDefault(e => e.SeasonNumber == mapping.Tvdb.Season && e.EpisodeNumber == mapping.Tvdb.Episode);

                    if (episode == null)
                    {
                        _logger.Debug("Information hasn't been added to TheTVDB yet, skipping.");
                        continue;
                    }

                    episode.SceneAbsoluteEpisodeNumber = mapping.Scene.Absolute;
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
            var ids = _xemProxy.GetXemSeriesIds();

            if (ids.Any())
            {
                _cache.Clear();
            }

            foreach (var id in ids)
            {
                _cache.Set(id.ToString(), true, TimeSpan.FromHours(1));
            }
        }

        public List<SceneMapping> GetSceneMappings()
        {
            var mappings = _xemProxy.GetSceneTvdbNames();

            return mappings.Where(m =>
            {
                int id;

                if (Int32.TryParse(m.Title, out id))
                {
                    _logger.Debug("Skipping all numeric name: {0} for {1}", m.Title, m.TvdbId);
                    return false;
                }

                return true;
            }).ToList();
        }

        public void Handle(SeriesUpdatedEvent message)
        {
            if (_cache.Count == 0)
            {
                RefreshCache();
            }

            if (!_cache.Find(message.Series.TvdbId.ToString()))
            {
                _logger.Debug("Scene numbering is not available for {0} [{1}]", message.Series.Title, message.Series.TvdbId);
                return;
            }

            PerformUpdate(message.Series);
        }

        public void Handle(SeriesRefreshStartingEvent message)
        {
            RefreshCache();
        }
    }
}
