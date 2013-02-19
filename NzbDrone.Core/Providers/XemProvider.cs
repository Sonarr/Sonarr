using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class XemProvider
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly XemCommunicationProvider _xemCommunicationProvider;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public XemProvider(SeriesProvider seriesProvider, EpisodeProvider episodeProvider,
                            XemCommunicationProvider xemCommunicationProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _xemCommunicationProvider = xemCommunicationProvider;
        }

        public XemProvider()
        {
        }

        public virtual void UpdateMappings()
        {
            _logger.Trace("Starting scene numbering update");
            try
            {
                var ids = _xemCommunicationProvider.GetXemSeriesIds();
                var series = _seriesProvider.GetAllSeries();
                var wantedSeries = series.Where(s => ids.Contains(s.SeriesId)).ToList();

                foreach(var ser in wantedSeries)
                {
                    PerformUpdate(ser);
                }

                _logger.Trace("Completed scene numbering update");
            }

            catch(Exception ex)
            {
                _logger.WarnException("Error updating Scene Mappings", ex);
                throw;
            }
        }

        public virtual void UpdateMappings(int seriesId)
        {
            var xemIds = _xemCommunicationProvider.GetXemSeriesIds();

            if (!xemIds.Contains(seriesId))
            {
                _logger.Trace("Xem doesn't have a mapping for this series: {0}", seriesId);
                return;
            }

            var series = _seriesProvider.GetSeries(seriesId);

            if (series == null)
            {
                _logger.Trace("Series could not be found: {0}", seriesId);
                return;
            }

            PerformUpdate(series);
        }

        public virtual void PerformUpdate(Series series)
        {
            _logger.Trace("Updating scene numbering mapping for: {0}", series.Title);
            try
            {
                var episodesToUpdate = new List<Episode>();
                var mappings = _xemCommunicationProvider.GetSceneTvdbMappings(series.SeriesId);

                if (mappings == null)
                {
                    _logger.Trace("Mappings for: {0} are null, skipping", series.Title);
                    return;
                }

                var episodes = _episodeProvider.GetEpisodeBySeries(series.SeriesId);

                foreach (var mapping in mappings)
                {
                    _logger.Trace("Setting scene numbering mappings for {0} S{1:00}E{2:00}", series.Title, mapping.Tvdb.Season, mapping.Tvdb.Episode);

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

                _logger.Trace("Committing scene numbering mappings to database for: {0}", series.Title);
                _episodeProvider.UpdateEpisodes(episodesToUpdate);

                _logger.Trace("Setting UseSceneMapping for {0}", series.Title);
                series.UseSceneNumbering = true;
                _seriesProvider.UpdateSeries(series);
            }

            catch (Exception ex)
            {
                _logger.WarnException("Error updating scene numbering mappings for: " + series, ex);
            }
        }
    }
}
