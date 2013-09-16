using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Providers
{
    public interface IXemProvider
    {
        void UpdateMappings();
        void UpdateMappings(int seriesId);
        void UpdateMappings(Series series);
        void PerformUpdate(Series series);
    }

    public class XemProvider : IXemProvider, IExecute<UpdateXemMappingsCommand>, IHandle<SeriesUpdatedEvent>, IHandleAsync<ApplicationStartedEvent>
    {
        private readonly IEpisodeService _episodeService;
        private readonly IXemCommunicationProvider _xemCommunicationProvider;
        private readonly ISeriesService _seriesService;
        private readonly ICached<bool> _cache;

        private static readonly Logger _logger =  NzbDroneLogger.GetLogger();

        public XemProvider(IEpisodeService episodeService,
                           IXemCommunicationProvider xemCommunicationProvider,
                           ISeriesService seriesService, ICacheManger cacheManger)
        {
            if (seriesService == null) throw new ArgumentNullException("seriesService");
            _episodeService = episodeService;
            _xemCommunicationProvider = xemCommunicationProvider;
            _seriesService = seriesService;
            _cache = cacheManger.GetCache<bool>(GetType());
        }

        public void UpdateMappings()
        {
            _logger.Trace("Starting scene numbering update");

            try
            {
                var ids = GetXemSeriesIds();
                var series = _seriesService.GetAllSeries();
                var wantedSeries = series.Where(s => ids.Contains(s.TvdbId)).ToList();

                foreach (var ser in wantedSeries)
                {
                    PerformUpdate(ser);
                }

                _logger.Trace("Completed scene numbering update");
            }

            catch (Exception ex)
            {
                _logger.WarnException("Error updating Scene Mappings", ex);
                throw;
            }
        }

        public void UpdateMappings(int seriesId)
        {
            var series = _seriesService.GetSeries(seriesId);

            if (series == null)
            {
                _logger.Trace("Series could not be found: {0}", seriesId);
                return;
            }
            
            UpdateMappings(series);
        }

        public void UpdateMappings(Series series)
        {
            if (!_cache.Find(series.TvdbId.ToString()))
            {
                _logger.Trace("Scene numbering is not available for {0} [{1}]", series.Title, series.TvdbId);
                return;
            }

            PerformUpdate(series);
        }

        public void PerformUpdate(Series series)
        {
            _logger.Trace("Updating scene numbering mapping for: {0}", series);
            try
            {
                var episodesToUpdate = new List<Episode>();
                var mappings = _xemCommunicationProvider.GetSceneTvdbMappings(series.TvdbId);

                if (mappings == null)
                {
                    _logger.Trace("Mappings for: {0} are null, skipping", series);
                    return;
                }

                var episodes = _episodeService.GetEpisodeBySeries(series.Id);

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
                    episodesToUpdate.Add(episode);
                }

                _logger.Trace("Committing scene numbering mappings to database for: {0}", series);
                _episodeService.UpdateEpisodes(episodesToUpdate);

                _logger.Trace("Setting UseSceneMapping for {0}", series);
                series.UseSceneNumbering = true;
                _seriesService.UpdateSeries(series);
            }

            catch (Exception ex)
            {
                _logger.ErrorException("Error updating scene numbering mappings for: " + series, ex);
            }
        }

        private List<int> GetXemSeriesIds()
        {
            _cache.Clear();

            var ids = _xemCommunicationProvider.GetXemSeriesIds();

            foreach (var id in ids)
            {
                _cache.Set(id.ToString(), true);
            }

            return ids;
        }

        public void Execute(UpdateXemMappingsCommand message)
        {
            if (message.SeriesId.HasValue)
            {
                UpdateMappings(message.SeriesId.Value);
            }
            else
            {
                UpdateMappings();
            }
        }

        public void Handle(SeriesUpdatedEvent message)
        {
            UpdateMappings(message.Series);
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            GetXemSeriesIds();
        }
    }
}
