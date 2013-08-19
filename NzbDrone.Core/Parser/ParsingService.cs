using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        LocalEpisode GetEpisodes(string filename, Series series);
        LocalEpisode GetEpisodes(string filename, Series series, bool sceneSource);
        Series GetSeries(string title);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvRageId);
    }

    public class ParsingService : IParsingService
    {
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;

        public ParsingService(IEpisodeService episodeService, ISeriesService seriesService, Logger logger)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _logger = logger;
        }

        public LocalEpisode GetEpisodes(string filename, Series series)
        {
            return GetEpisodes(filename, series, false);
        }

        public LocalEpisode GetEpisodes(string filename, Series series, bool sceneSource)
        {
            var parsedEpisodeInfo = Parser.ParsePath(filename);

            if (parsedEpisodeInfo == null)
            {
                return null;
            }

            var episodes = GetEpisodes(parsedEpisodeInfo, series, sceneSource);

            if (!episodes.Any())
            {
                return null;
            }

            return new LocalEpisode
            {
                Series = series,
                Quality = parsedEpisodeInfo.Quality,
                Episodes = episodes,
                Path = filename,
                ParsedEpisodeInfo = parsedEpisodeInfo
            };
        }

        public Series GetSeries(string title)
        {
            var searchTitle = title;

            var parsedEpisodeInfo = Parser.ParseTitle(title);

            if (parsedEpisodeInfo != null)
            {
                searchTitle = parsedEpisodeInfo.SeriesTitle;
            }

            return _seriesService.FindByTitle(searchTitle);
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvRageId)
        {
            var remoteEpisode = new RemoteEpisode
                {
                    ParsedEpisodeInfo = parsedEpisodeInfo,
                };

            var series = _seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitle);

            if (series == null && tvRageId > 0)
            {
                series = _seriesService.FindByTvRageId(tvRageId);
                //TODO: If series is found by TvRageId, we should report it as a scene naming exception, since it will fail to import
            }

            if (series == null)
            {
                _logger.Trace("No matching series {0}", parsedEpisodeInfo.SeriesTitle);
                return remoteEpisode;
            }

            remoteEpisode.Series = series;
            remoteEpisode.Episodes = GetEpisodes(parsedEpisodeInfo, series, true);

            return remoteEpisode;
        }

        private List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, bool sceneSource)
        {
            var result = new List<Episode>();

            if (parsedEpisodeInfo.AirDate.HasValue)
            {
                if (series.SeriesType == SeriesTypes.Standard)
                {
                    _logger.Warn("Found daily-style episode for non-daily series: {0}.", series);
                    return new List<Episode>();
                }
                
                var episodeInfo = _episodeService.GetEpisode(series.Id, parsedEpisodeInfo.AirDate.Value);

                if (episodeInfo != null)
                {
                    result.Add(episodeInfo);
                }

                return result;
            }

            if (parsedEpisodeInfo.EpisodeNumbers == null)
                return result;

            foreach (var episodeNumber in parsedEpisodeInfo.EpisodeNumbers)
            {
                Episode episodeInfo = null;

                if (series.UseSceneNumbering && sceneSource)
                {
                    episodeInfo = _episodeService.FindEpisode(series.Id, parsedEpisodeInfo.SeasonNumber, episodeNumber, true);

                    if (episodeInfo != null)
                    {
                        _logger.Info("Using Scene to TVDB Mapping for: {0} - Scene: {1}x{2:00} - TVDB: {3}x{4:00}",
                                    series.Title,
                                    episodeInfo.SceneSeasonNumber,
                                    episodeInfo.SceneEpisodeNumber,
                                    episodeInfo.SeasonNumber,
                                    episodeInfo.EpisodeNumber);
                    }
                }

                if (episodeInfo == null)
                {
                    episodeInfo = _episodeService.GetEpisode(series.Id, parsedEpisodeInfo.SeasonNumber, episodeNumber);
                }

                if (episodeInfo != null)
                {
                    result.Add(episodeInfo);
                }
                else
                {
                    _logger.Debug("Unable to find {0}", parsedEpisodeInfo);
                }
            }

            return result;
        }
    }
}