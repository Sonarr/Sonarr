using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        LocalEpisode GetEpisodes(string fileName, Series series);
        Series GetSeries(string title);
        RemoteEpisode Map(ReportInfo indexerParseResult);
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


        public LocalEpisode GetEpisodes(string fileName, Series series)
        {
            var parseResult = Parser.ParseTitle(fileName);

            if (parseResult == null)
            {
                return null;
            }

            var episodes = GetEpisodesByParseResult(parseResult, series);

            if (!episodes.Any())
            {
                return null;
            }

            return new LocalEpisode
                {
                    Quality = parseResult.Quality,
                    Episodes = episodes,
                };
        }

        public Series GetSeries(string title)
        {
            var searchTitle = title;

            var parseResult = Parser.ParseTitle(title);

            if (parseResult != null)
            {
                searchTitle = parseResult.SeriesTitle;
            }

            return _seriesService.FindByTitle(searchTitle);
        }

        public RemoteEpisode Map(ReportInfo indexerParseResult)
        {
            throw new NotImplementedException();
        }

        private List<Episode> GetEpisodesByParseResult(ParsedEpisodeInfo parseResult, Series series)
        {
            var result = new List<Episode>();

            if (parseResult.AirDate.HasValue)
            {
                if (series.SeriesType == SeriesTypes.Standard)
                {
                    //Todo: Collect this as a Series we want to treat as a daily series, or possible parsing error
                    _logger.Warn("Found daily-style episode for non-daily series: {0}. {1}", series.Title, parseResult.OriginalString);
                    return new List<Episode>();
                }

                var episodeInfo = _episodeService.GetEpisode(series.Id, parseResult.AirDate.Value);

                if (episodeInfo != null)
                {
                    result.Add(episodeInfo);
                }

                return result;
            }

            if (parseResult.EpisodeNumbers == null)
                return result;

            foreach (var episodeNumber in parseResult.EpisodeNumbers)
            {
                Episode episodeInfo = null;

                if (series.UseSceneNumbering && parseResult.SceneSource)
                {
                    episodeInfo = _episodeService.GetEpisode(series.Id, parseResult.SeasonNumber, episodeNumber, true);
                }

                if (episodeInfo == null)
                {
                    episodeInfo = _episodeService.GetEpisode(series.Id, parseResult.SeasonNumber, episodeNumber);
                    if (episodeInfo == null && parseResult.AirDate != null)
                    {
                        episodeInfo = _episodeService.GetEpisode(series.Id, parseResult.AirDate.Value);
                    }
                }

                if (episodeInfo != null)
                {
                    result.Add(episodeInfo);

                    if (series.UseSceneNumbering)
                    {
                        _logger.Info("Using Scene to TVDB Mapping for: {0} - Scene: {1}x{2:00} - TVDB: {3}x{4:00}",
                                    series.Title,
                                    episodeInfo.SceneSeasonNumber,
                                    episodeInfo.SceneEpisodeNumber,
                                    episodeInfo.SeasonNumber,
                                    episodeInfo.EpisodeNumber);
                    }
                }
                else
                {
                    _logger.Debug("Unable to find {0}", parseResult);
                }
            }

            return result;
        }
    }
}