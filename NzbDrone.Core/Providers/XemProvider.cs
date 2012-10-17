using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Ninject;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class XemProvider
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly XemCommunicationProvider _xemCommunicationProvider;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [Inject]
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
                var wantedSeries = series.Where(s => ids.Contains(s.SeriesId));

                foreach(var ser in wantedSeries)
                {
                    _logger.Trace("Updating scene numbering mapping for: {0}", ser.Title);
                    try
                    {
                        var episodesToUpdate = new List<Episode>();
                        var mappings = _xemCommunicationProvider.GetSceneTvdbMappings(ser.SeriesId);

                        if (mappings == null)
                        {
                            _logger.Trace("Mappings for: {0} are null, skipping", ser.Title);
                            continue;
                        }

                        foreach(var mapping in mappings)
                        {
                            _logger.Trace("Setting scene numbering mappings for {0} S{1:00}E{2:00}", ser.Title, mapping.Tvdb.Season, mapping.Tvdb.Episode);

                            var episode = _episodeProvider.GetEpisode(ser.SeriesId, mapping.Tvdb.Season, mapping.Tvdb.Episode);
                            episode.AbsoluteEpisodeNumber = mapping.Scene.Absolute;
                            episode.SceneSeasonNumber = mapping.Scene.Season;
                            episode.SceneEpisodeNumber = mapping.Scene.Episode;
                            episodesToUpdate.Add(episode);
                        }
                        
                        _logger.Trace("Committing scene numbering mappings to database for: {0}", ser.Title);
                        _episodeProvider.UpdateEpisodes(episodesToUpdate);
                    }

                    catch(Exception ex)
                    {
                        _logger.WarnException("Error updating scene numbering mappings for: " + ser, ex);
                    }
                }

                _logger.Trace("Completed scene numbering update");
            }

            catch(Exception ex)
            {
                _logger.WarnException("Error updating Scene Mappings", ex);
                throw;
            }
        }
    }
}
