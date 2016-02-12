using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        LocalEpisode GetLocalEpisode(string filename, Series series);
        LocalEpisode GetLocalEpisode(string filename, Series series, ParsedEpisodeInfo folderInfo, bool sceneSource);
        Series GetSeries(string title);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, int tvRageId, SearchCriteriaBase searchCriteria = null);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int seriesId, IEnumerable<int> episodeIds);
        List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, bool sceneSource, SearchCriteriaBase searchCriteria = null);
        ParsedEpisodeInfo ParseSpecialEpisodeTitle(string title, int tvdbId, int tvRageId, SearchCriteriaBase searchCriteria = null);
    }

    public class ParsingService : IParsingService
    {
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesService _seriesService;
        private readonly ISceneMappingService _sceneMappingService;
        private readonly Logger _logger;

        public ParsingService(IEpisodeService episodeService,
                              ISeriesService seriesService,
                              ISceneMappingService sceneMappingService,
                              Logger logger)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _sceneMappingService = sceneMappingService;
            _logger = logger;
        }

        public LocalEpisode GetLocalEpisode(string filename, Series series)
        {
            return GetLocalEpisode(filename, series, null, false);
        }

        public LocalEpisode GetLocalEpisode(string filename, Series series, ParsedEpisodeInfo folderInfo, bool sceneSource)
        {
            ParsedEpisodeInfo parsedEpisodeInfo;

            if (folderInfo != null)
            {
                parsedEpisodeInfo = folderInfo.JsonClone();
                parsedEpisodeInfo.Quality = QualityParser.ParseQuality(Path.GetFileName(filename));
            }

            else
            {
                parsedEpisodeInfo = Parser.ParsePath(filename);
            }

            if (parsedEpisodeInfo == null || parsedEpisodeInfo.IsPossibleSpecialEpisode)
            {
                var title = Path.GetFileNameWithoutExtension(filename);
                var specialEpisodeInfo = ParseSpecialEpisodeTitle(title, series);

                if (specialEpisodeInfo != null)
                {
                    parsedEpisodeInfo = specialEpisodeInfo;
                }
            }

            if (parsedEpisodeInfo == null)
            {
                return null;
            }

            var episodes = GetEpisodes(parsedEpisodeInfo, series, sceneSource);

            return new LocalEpisode
            {
                Series = series,
                Quality = parsedEpisodeInfo.Quality,
                Episodes = episodes,
                Path = filename,
                ParsedEpisodeInfo = parsedEpisodeInfo,
                ExistingFile = series.Path.IsParentPath(filename)
            };
        }

        public Series GetSeries(string title)
        {
            var parsedEpisodeInfo = Parser.ParseTitle(title);

            if (parsedEpisodeInfo == null)
            {
                return _seriesService.FindByTitle(title);
            }

            var series = _seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitle);

            if (series == null)
            {
                series = _seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitleInfo.TitleWithoutYear,
                                                    parsedEpisodeInfo.SeriesTitleInfo.Year);
            }

            return series;
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, int tvRageId, SearchCriteriaBase searchCriteria = null)
        {
            var remoteEpisode = new RemoteEpisode
                {
                    ParsedEpisodeInfo = parsedEpisodeInfo,
                };

            var series = GetSeries(parsedEpisodeInfo, tvdbId, tvRageId, searchCriteria);

            if (series == null)
            {
                return remoteEpisode;
            }

            remoteEpisode.Series = series;
            remoteEpisode.Episodes = GetEpisodes(parsedEpisodeInfo, series, true, searchCriteria);

            return remoteEpisode;
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int seriesId, IEnumerable<int> episodeIds)
        {
            return new RemoteEpisode
                   {
                       ParsedEpisodeInfo = parsedEpisodeInfo,
                       Series = _seriesService.GetSeries(seriesId),
                       Episodes = _episodeService.GetEpisodes(episodeIds)
                   };
        }

        public List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, bool sceneSource, SearchCriteriaBase searchCriteria = null)
        {
            var result = new List<Episode>();

            if (parsedEpisodeInfo.FullSeason)
            {
                return _episodeService.GetEpisodesBySeason(series.Id, parsedEpisodeInfo.SeasonNumber);
            }

            if (parsedEpisodeInfo.IsDaily)
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

            if (parsedEpisodeInfo.IsAbsoluteNumbering)
            {
                var sceneSeasonNumber = _sceneMappingService.GetSceneSeasonNumber(parsedEpisodeInfo.SeriesTitle);

                foreach (var absoluteEpisodeNumber in parsedEpisodeInfo.AbsoluteEpisodeNumbers)
                {
                    Episode episode = null;

                    if (parsedEpisodeInfo.Special)
                    {
                        episode = _episodeService.FindEpisode(series.Id, 0, absoluteEpisodeNumber);
                    }

                    else if (sceneSource)
                    {
                        // Is there a reason why we excluded season 1 from this handling before?
                        // Might have something to do with the scene name to season number check
                        // If this needs to be reverted tests will need to be added
                        if (sceneSeasonNumber.HasValue)
                        {
                            var episodes = _episodeService.FindEpisodesBySceneNumbering(series.Id, sceneSeasonNumber.Value, absoluteEpisodeNumber);

                            if (episodes.Count == 1)
                            {
                                episode = episodes.First();
                            }

                            if (episode == null)
                            {
                                episode = _episodeService.FindEpisode(series.Id, sceneSeasonNumber.Value, absoluteEpisodeNumber);
                            }
                        }

                        else
                        {
                            episode = _episodeService.FindEpisodeBySceneNumbering(series.Id, absoluteEpisodeNumber);
                        }
                    }

                    if (episode == null)
                    {
                        episode = _episodeService.FindEpisode(series.Id, absoluteEpisodeNumber);
                    }
                   
                    if (episode != null)
                    {
                        _logger.Debug("Using absolute episode number {0} for: {1} - TVDB: {2}x{3:00}",
                                    absoluteEpisodeNumber,
                                    series.Title,
                                    episode.SeasonNumber,
                                    episode.EpisodeNumber);

                        result.Add(episode);
                    }
                }

                return result;
            }

            if (parsedEpisodeInfo.EpisodeNumbers == null)
                return result;

            foreach (var episodeNumber in parsedEpisodeInfo.EpisodeNumbers)
            {
                if (series.UseSceneNumbering && sceneSource)
                {
                    List<Episode> episodes = new List<Episode>();

                    if (searchCriteria != null)
                    {
                        episodes = searchCriteria.Episodes.Where(e => e.SceneSeasonNumber == parsedEpisodeInfo.SeasonNumber &&
                                                                          e.SceneEpisodeNumber == episodeNumber).ToList();
                    }

                    if (!episodes.Any())
                    {
                        episodes = _episodeService.FindEpisodesBySceneNumbering(series.Id, parsedEpisodeInfo.SeasonNumber, episodeNumber);
                    }

                    if (episodes != null && episodes.Any())
                    {
                        _logger.Debug("Using Scene to TVDB Mapping for: {0} - Scene: {1}x{2:00} - TVDB: {3}",
                                    series.Title,
                                    episodes.First().SceneSeasonNumber,
                                    episodes.First().SceneEpisodeNumber,
                                    string.Join(", ", episodes.Select(e => string.Format("{0}x{1:00}", e.SeasonNumber, e.EpisodeNumber))));

                        result.AddRange(episodes);
                        continue;
                    }
                }

                Episode episodeInfo = null;

                if (searchCriteria != null)
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

        public ParsedEpisodeInfo ParseSpecialEpisodeTitle(string title, int tvdbId, int tvRageId, SearchCriteriaBase searchCriteria = null)
        {
            if (searchCriteria != null)
            {
                if (tvdbId == 0)
                    tvdbId = _sceneMappingService.FindTvdbId(title) ?? 0;

                if (tvdbId != 0 && tvdbId == searchCriteria.Series.TvdbId)
                {
                    return ParseSpecialEpisodeTitle(title, searchCriteria.Series);
                }

                if (tvRageId != 0 && tvRageId == searchCriteria.Series.TvRageId)
                {
                    return ParseSpecialEpisodeTitle(title, searchCriteria.Series);
                }
            }

            var series = GetSeries(title);

            if (series == null)
            {
                series = _seriesService.FindByTitleInexact(title);
            }

            if (series == null && tvdbId > 0)
            {
                series = _seriesService.FindByTvdbId(tvdbId);
            }

            if (series == null && tvRageId > 0)
            {
                series = _seriesService.FindByTvRageId(tvRageId);
            }

            if (series == null)
            {
                _logger.Debug("No matching series {0}", title);
                return null;
            }

            return ParseSpecialEpisodeTitle(title, series);
        }

        private ParsedEpisodeInfo ParseSpecialEpisodeTitle(string title, Series series)
        {
            // find special episode in series season 0
            var episode = _episodeService.FindEpisodeByTitle(series.Id, 0, title);

            if (episode != null)
            {
                // create parsed info from tv episode
                var info = new ParsedEpisodeInfo();
                info.SeriesTitle = series.Title;
                info.SeriesTitleInfo = new SeriesTitleInfo();
                info.SeriesTitleInfo.Title = info.SeriesTitle;
                info.SeasonNumber = episode.SeasonNumber;
                info.EpisodeNumbers = new int[1] { episode.EpisodeNumber };
                info.FullSeason = false;
                info.Quality = QualityParser.ParseQuality(title);
                info.ReleaseGroup = Parser.ParseReleaseGroup(title);
                info.Language = Parser.ParseLanguage(title);
                info.Special = true;

                _logger.Debug("Found special episode {0} for title '{1}'", info, title);
                return info;
            }

            return null;
        }

        private Series GetSeries(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, int tvRageId, SearchCriteriaBase searchCriteria)
        {
            Series series = null;

            var sceneMappingTvdbId = _sceneMappingService.FindTvdbId(parsedEpisodeInfo.SeriesTitle);
            if (sceneMappingTvdbId.HasValue)
            {
                if (searchCriteria != null && searchCriteria.Series.TvdbId == sceneMappingTvdbId.Value)
                {
                    return searchCriteria.Series;
                }

                series = _seriesService.FindByTvdbId(sceneMappingTvdbId.Value);

                if (series == null)
                {
                    _logger.Debug("No matching series {0}", parsedEpisodeInfo.SeriesTitle);
                    return null;
                }

                return series;
            }

            if (searchCriteria != null)
            {
                if (searchCriteria.Series.CleanTitle == parsedEpisodeInfo.SeriesTitle.CleanSeriesTitle())
                {
                    return searchCriteria.Series;
                }

                if (tvdbId > 0 && tvdbId == searchCriteria.Series.TvdbId)
                {
                    //TODO: If series is found by TvdbId, we should report it as a scene naming exception, since it will fail to import
                    return searchCriteria.Series;
                }

                if (tvRageId > 0 && tvRageId == searchCriteria.Series.TvRageId)
                {
                    //TODO: If series is found by TvRageId, we should report it as a scene naming exception, since it will fail to import
                    return searchCriteria.Series;
                }
            }

            series = _seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitle);

            if (series == null && tvdbId > 0)
            {
                //TODO: If series is found by TvdbId, we should report it as a scene naming exception, since it will fail to import
                series = _seriesService.FindByTvdbId(tvdbId);
            }

            if (series == null && tvRageId > 0)
            {
                //TODO: If series is found by TvRageId, we should report it as a scene naming exception, since it will fail to import
                series = _seriesService.FindByTvRageId(tvRageId);
            }

            if (series == null)
            {
                _logger.Debug("No matching series {0}", parsedEpisodeInfo.SeriesTitle);
                return null;
            }

            return series;
        }

        private Episode GetDailyEpisode(Series series, string airDate, SearchCriteriaBase searchCriteria)
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