using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        ParsedEpisodeInfo ParseTitle(string title, Series series, bool sceneSource);
        LocalEpisode GetEpisodes(string filename, Series series, bool sceneSource);
        Series GetSeries(string title);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvRageId, SearchCriteriaBase searchCriteria = null);
        List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, bool sceneSource, SearchCriteriaBase searchCriteria = null);
    }

    public class ParsingService : IParsingService
    {
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesService _seriesService;
        private readonly IDiskProvider _diskProvider;
        private readonly ISceneMappingService _sceneMappingService;
        private readonly Logger _logger;

        public ParsingService(IEpisodeService episodeService,
                              ISeriesService seriesService,
                              IDiskProvider diskProvider,
                              ISceneMappingService sceneMappingService,
                              Logger logger)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _diskProvider = diskProvider;
            _sceneMappingService = sceneMappingService;
            _logger = logger;
        }


        public ParsedEpisodeInfo ParseTitle(string title, Series series, bool sceneSource)
        {
            ParsedEpisodeInfo info = Parser.ParseTitle(title);

            try
            {
                if (series == null)
                {
                    // find series if we dont have it already
                    // the series search is able to find a title with an inexact match even though there is release metadata on it
                    series = _seriesService.FindByTitleInexact((info != null) ? info.SeriesTitle : title);
                    if (series == null)
                    {
                        // no series
                        _logger.Info("Could not find Series for {0}", title);
                        return null;
                    }
                }

                // validate parsed info using backend, if we have low confidence that it is correct we will do an episode title search
                //
                // if no episode numbers we probably have a low confidence parse,
                //    a "S01" could mean a FullSeason or a single named episode in season 1 with trailing episode title:
                //              <series>.S01.HDTV-Releaser                 <-- FullSeason release
                //              <series>.S01.Holiday.Christmas.Special     <-- Special episode release with Season# and title
                //
                // we also call GetEpisodes() here to make sure we have these episode numbers in our local episode DB, 
                //   if not then they may be numbered incorrectly and a title search may find the correct episode
                if (info == null || info.EpisodeNumbers.Length == 0 || !GetEpisodes(info, series, sceneSource).Any())
                {
                    // find episode by title using a fuzzy match through episode service
                    // this will handle most special episodes and incorrect matchess
                    var episodes = _episodeService.FindEpisodeByNameInexact(series.Id, title).ToList();
                    if (episodes.Count == 1)
                    {
                        var episode = episodes.Single();

                        // created parsed info from tv episode that we found
                        info = new ParsedEpisodeInfo();
                        info.SeriesTitle = series.Title;
                        info.SeriesTitleInfo = new SeriesTitleInfo();
                        info.SeriesTitleInfo.Title = info.SeriesTitle;
                        info.SeasonNumber = episode.SeasonNumber;
                        info.EpisodeNumbers = new int[1] { episode.EpisodeNumber };
                        info.FullSeason = false;
                        info.Quality = QualityParser.ParseQuality(title);
                        info.ReleaseGroup = Parser.ParseReleaseGroup(title);

                        _logger.Info("Found episode {0} for title '{1}'", info, title);
                    }
                    else if (episodes.Count > 1)
                    {
                        // more than one episode with this name?
                        _logger.Warn("Found multiple episodes in series {0} for title '{1}'", series.Title, title);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException("An error has occurred while trying to parse " + title, e);
            }

            return info;
        }

        public LocalEpisode GetEpisodes(string filename, Series series, bool sceneSource)
        {
            string title = System.IO.Path.GetFileNameWithoutExtension(filename);

            var parsedEpisodeInfo = this.ParseTitle(title, series, sceneSource);

            if (parsedEpisodeInfo == null)
            {
                return null;
            }

            var episodes = GetEpisodes(parsedEpisodeInfo, series, sceneSource);

            if (!episodes.Any())
            {
                _logger.Trace("No matching episodes found for: {0}", parsedEpisodeInfo);
                return null;
            }

            return new LocalEpisode
            {
                Series = series,
                Quality = parsedEpisodeInfo.Quality,
                Episodes = episodes,
                Path = filename,
                ParsedEpisodeInfo = parsedEpisodeInfo,
                ExistingFile = _diskProvider.IsParent(series.Path, filename)
            };
        }

        public Series GetSeries(string title)
        {
            var parsedEpisodeInfo = Parser.ParseTitle(title);
            if (parsedEpisodeInfo == null)
            {
                return _seriesService.FindByTitleInexact(title);
            }

            var series = _seriesService.FindByTitleInexact(parsedEpisodeInfo.SeriesTitle);

            if (series == null)
            {
                series = _seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitleInfo.TitleWithoutYear,
                                                    parsedEpisodeInfo.SeriesTitleInfo.Year);
            }

            return series;
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvRageId, SearchCriteriaBase searchCriteria = null)
        {
            var remoteEpisode = new RemoteEpisode
                {
                    ParsedEpisodeInfo = parsedEpisodeInfo,
                };

            var series = searchCriteria == null ? GetSeries(parsedEpisodeInfo, tvRageId) :
                                                  GetSeries(parsedEpisodeInfo, tvRageId, searchCriteria);

            if (series == null)
            {
                return remoteEpisode;
            }

            remoteEpisode.Series = series;
            remoteEpisode.Episodes = GetEpisodes(parsedEpisodeInfo, series, true, searchCriteria);

            return remoteEpisode;
        }

        public List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, bool sceneSource, SearchCriteriaBase searchCriteria = null)
        {
            var result = new List<Episode>();

            if (parsedEpisodeInfo.FullSeason)
            {
                return _episodeService.GetEpisodesBySeason(series.Id, parsedEpisodeInfo.SeasonNumber);
            }

            if (parsedEpisodeInfo.IsDaily())
            {
                if (series.SeriesType == SeriesTypes.Standard)
                {
                    _logger.Warn("Found daily-style episode for non-daily series: {0}.", series);
                    return result;
                }

                var episodeInfo = GetDailyEpisode(series, parsedEpisodeInfo.AirDate, searchCriteria);

                if (episodeInfo != null)
                {
                    result.Add(episodeInfo);
                }

                return result;
            }

            if (parsedEpisodeInfo.IsAbsoluteNumbering())
            {
                foreach (var absoluteEpisodeNumber in parsedEpisodeInfo.AbsoluteEpisodeNumbers)
                {
                    var episodeInfo = _episodeService.FindEpisode(series.Id, absoluteEpisodeNumber);

                    if (episodeInfo != null)
                    {
                        _logger.Info("Using absolute episode number {0} for: {1} - TVDB: {2}x{3:00}",
                                    absoluteEpisodeNumber,
                                    series.Title,
                                    episodeInfo.SeasonNumber,
                                    episodeInfo.EpisodeNumber);
                        result.Add(episodeInfo);
                    }
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
                    if (searchCriteria != null)
                    {
                        episodeInfo = searchCriteria.Episodes.SingleOrDefault(e => e.SceneSeasonNumber == parsedEpisodeInfo.SeasonNumber &&
                                                                          e.SceneEpisodeNumber == episodeNumber);
                    }

                    if (episodeInfo == null)
                    {
                        episodeInfo = _episodeService.FindEpisode(series.Id, parsedEpisodeInfo.SeasonNumber, episodeNumber, true);
                    }

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

                if (episodeInfo == null && searchCriteria != null)
                {
                    episodeInfo = searchCriteria.Episodes.SingleOrDefault(e => e.SeasonNumber == parsedEpisodeInfo.SeasonNumber &&
                                                                          e.EpisodeNumber == episodeNumber);
                }

                if (episodeInfo == null)
                {
                    episodeInfo = _episodeService.FindEpisode(series.Id, parsedEpisodeInfo.SeasonNumber, episodeNumber);
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

        private Series GetSeries(ParsedEpisodeInfo parsedEpisodeInfo, int tvRageId, SearchCriteriaBase searchCriteria)
        {
            var tvdbId = _sceneMappingService.GetTvDbId(parsedEpisodeInfo.SeriesTitle);

            if (tvdbId.HasValue)
            {
                if (searchCriteria.Series.TvdbId == tvdbId)
                {
                    return searchCriteria.Series;
                }
            }

            if (parsedEpisodeInfo.SeriesTitle.CleanSeriesTitle() == searchCriteria.Series.CleanTitle)
            {
                return searchCriteria.Series;
            }

            if (tvRageId == searchCriteria.Series.TvRageId)
            {
                //TODO: If series is found by TvRageId, we should report it as a scene naming exception, since it will fail to import
                return searchCriteria.Series;
            }

            return GetSeries(parsedEpisodeInfo, tvRageId);
        }

        private Series GetSeries(ParsedEpisodeInfo parsedEpisodeInfo, int tvRageId)
        {
            var series = _seriesService.FindByTitleInexact(parsedEpisodeInfo.SeriesTitle);

            if (series == null && tvRageId > 0)
            {
                //TODO: If series is found by TvRageId, we should report it as a scene naming exception, since it will fail to import
                series = _seriesService.FindByTvRageId(tvRageId);
            }

            if (series == null)
            {
                _logger.Trace("No matching series {0}", parsedEpisodeInfo.SeriesTitle);
                return null;
            }

            return series;
        }

        private Episode GetDailyEpisode(Series series, String airDate, SearchCriteriaBase searchCriteria)
        {
            Episode episodeInfo = null;

            if (searchCriteria != null)
            {
                episodeInfo = searchCriteria.Episodes.SingleOrDefault(
                    e => e.AirDate == airDate);
            }

            if (episodeInfo == null)
            {
                episodeInfo = _episodeService.FindEpisode(series.Id, airDate);
            }

            return episodeInfo;
        }
    }
}