using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.LlmMatching;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        Series GetSeries(string title);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, int tvRageId, string imdbId, SearchCriteriaBase searchCriteria = null);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, Series series);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int seriesId, IEnumerable<int> episodeIds);
        List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, bool sceneSource, SearchCriteriaBase searchCriteria = null);
        ParsedEpisodeInfo ParseSpecialEpisodeTitle(ParsedEpisodeInfo parsedEpisodeInfo, string releaseTitle, int tvdbId, int tvRageId, string imdbId, SearchCriteriaBase searchCriteria = null);
        ParsedEpisodeInfo ParseSpecialEpisodeTitle(ParsedEpisodeInfo parsedEpisodeInfo, string releaseTitle, Series series);
    }

    public class ParsingService(
        IEpisodeService episodeService,
        ISeriesService seriesService,
        ISceneMappingService sceneMappingService,
        ILlmSeriesMatchingService llmMatchingService,
        Logger logger) : IParsingService
    {
        public Series GetSeries(string title)
        {
            var parsedEpisodeInfo = Parser.ParseTitle(title);

            if (parsedEpisodeInfo == null)
            {
                return seriesService.FindByTitle(title);
            }

            var tvdbId = sceneMappingService.FindTvdbId(parsedEpisodeInfo.SeriesTitle, parsedEpisodeInfo.ReleaseTitle, parsedEpisodeInfo.SeasonNumber);

            if (tvdbId.HasValue)
            {
                return seriesService.FindByTvdbId(tvdbId.Value);
            }

            var series = seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitle);

            if (series == null && parsedEpisodeInfo.SeriesTitleInfo.AllTitles != null)
            {
                series = GetSeriesByAllTitles(parsedEpisodeInfo);
            }

            if (series == null)
            {
                series = seriesService.FindByTitle(
                    parsedEpisodeInfo.SeriesTitleInfo.TitleWithoutYear,
                    parsedEpisodeInfo.SeriesTitleInfo.Year);
            }

            // LLM FALLBACK - Try LLM matching if traditional methods failed
            if (series == null && llmMatchingService.IsEnabled)
            {
                series = TryLlmMatching(parsedEpisodeInfo, title);
            }

            return series;
        }

        private Series TryLlmMatching(ParsedEpisodeInfo parsedEpisodeInfo, string originalTitle)
        {
            if (!llmMatchingService.IsEnabled)
            {
                return null;
            }

            try
            {
                logger.Debug(
                    "Traditional matching failed for '{0}', attempting LLM matching",
                    parsedEpisodeInfo?.SeriesTitle ?? originalTitle);

                var availableSeries = seriesService.GetAllSeries();

                if (!availableSeries.Any())
                {
                    logger.Debug("No series in library for LLM matching");
                    return null;
                }

                var matchTask = parsedEpisodeInfo != null
                    ? llmMatchingService.TryMatchSeriesAsync(parsedEpisodeInfo, availableSeries)
                    : llmMatchingService.TryMatchSeriesAsync(originalTitle, availableSeries);

                var result = matchTask.GetAwaiter().GetResult();

                if (result?.IsSuccessfulMatch == true)
                {
                    logger.Info(
                        "LLM matched '{0}' to '{1}' (TVDB: {2}) with {3:P0} confidence",
                        parsedEpisodeInfo?.SeriesTitle ?? originalTitle,
                        result.Series.Title,
                        result.Series.TvdbId,
                        result.Confidence);

                    return result.Series;
                }

                if (result?.Alternatives?.Any() == true)
                {
                    logger.Debug(
                        "LLM found potential matches for '{0}' but confidence too low. Alternatives: {1}",
                        parsedEpisodeInfo?.SeriesTitle ?? originalTitle,
                        string.Join(", ", result.Alternatives.Select(a => $"{a.Series.Title} ({a.Confidence:P0})")));
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.Error(
                    ex,
                    "Error during LLM series matching for '{0}'",
                    parsedEpisodeInfo?.SeriesTitle ?? originalTitle);

                return null;
            }
        }

        private Series GetSeriesByAllTitles(ParsedEpisodeInfo parsedEpisodeInfo)
        {
            Series foundSeries = null;
            int? foundTvdbId = null;

            foreach (var title in parsedEpisodeInfo.SeriesTitleInfo.AllTitles)
            {
                var series = seriesService.FindByTitle(title);
                var tvdbId = series?.TvdbId;

                if (series == null)
                {
                    tvdbId = sceneMappingService.FindTvdbId(title, parsedEpisodeInfo.ReleaseTitle, parsedEpisodeInfo.SeasonNumber);
                }

                if (!tvdbId.HasValue)
                {
                    logger.Trace("Title {0} not matching any series.", title);
                    continue;
                }

                if (foundTvdbId.HasValue && tvdbId != foundTvdbId)
                {
                    logger.Trace("Title {0} both matches tvdbid {1} and {2}, no series selected.", parsedEpisodeInfo.SeriesTitle, foundTvdbId, tvdbId);
                    return null;
                }

                if (foundSeries == null)
                {
                    foundSeries = series;
                }

                foundTvdbId = tvdbId;
            }

            if (foundSeries == null && foundTvdbId.HasValue)
            {
                foundSeries = seriesService.FindByTvdbId(foundTvdbId.Value);
            }

            return foundSeries;
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, int tvRageId, string imdbId, SearchCriteriaBase searchCriteria = null)
        {
            return Map(parsedEpisodeInfo, tvdbId, tvRageId, imdbId, null, searchCriteria);
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, Series series)
        {
            return Map(parsedEpisodeInfo, 0, 0, null, series, null);
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int seriesId, IEnumerable<int> episodeIds)
        {
            return new RemoteEpisode
            {
                ParsedEpisodeInfo = parsedEpisodeInfo,
                Series = seriesService.GetSeries(seriesId),
                Episodes = episodeService.GetEpisodes(episodeIds)
            };
        }

        private RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, int tvRageId, string imdbId, Series series, SearchCriteriaBase searchCriteria)
        {
            var sceneMapping = sceneMappingService.FindSceneMapping(parsedEpisodeInfo.SeriesTitle, parsedEpisodeInfo.ReleaseTitle, parsedEpisodeInfo.SeasonNumber);

            var remoteEpisode = new RemoteEpisode
            {
                ParsedEpisodeInfo = parsedEpisodeInfo,
                SceneMapping = sceneMapping,
                MappedSeasonNumber = parsedEpisodeInfo.SeasonNumber
            };

            var sceneSource = true;

            if (sceneMapping != null)
            {
                if (sceneMapping.SeasonNumber.HasValue && sceneMapping.SeasonNumber.Value >= 0 &&
                    sceneMapping.SceneSeasonNumber <= parsedEpisodeInfo.SeasonNumber)
                {
                    remoteEpisode.MappedSeasonNumber += sceneMapping.SeasonNumber.Value - sceneMapping.SceneSeasonNumber.Value;
                }

                if (sceneMapping.SceneOrigin == "tvdb")
                {
                    sceneSource = false;
                }
                else if (sceneMapping.Type == "XemService" &&
                         sceneMapping.SceneSeasonNumber.NonNegative().HasValue &&
                         parsedEpisodeInfo.SeasonNumber == 1 &&
                         sceneMapping.SceneSeasonNumber != parsedEpisodeInfo.SeasonNumber)
                {
                    remoteEpisode.MappedSeasonNumber = sceneMapping.SceneSeasonNumber.Value;
                }
            }

            if (series == null)
            {
                var seriesMatch = FindSeries(parsedEpisodeInfo, tvdbId, tvRageId, imdbId, sceneMapping, searchCriteria);

                if (seriesMatch != null)
                {
                    series = seriesMatch.Series;
                    remoteEpisode.SeriesMatchType = seriesMatch.MatchType;
                }
            }

            if (series != null)
            {
                remoteEpisode.Series = series;

                if (ValidateParsedEpisodeInfo.ValidateForSeriesType(parsedEpisodeInfo, series))
                {
                    remoteEpisode.Episodes = GetEpisodes(parsedEpisodeInfo, series, remoteEpisode.MappedSeasonNumber, sceneSource, searchCriteria);
                }
            }

            remoteEpisode.Languages = parsedEpisodeInfo.Languages;

            if (remoteEpisode.Episodes == null)
            {
                remoteEpisode.Episodes = new List<Episode>();
            }

            if (searchCriteria != null)
            {
                var requestedEpisodes = searchCriteria.Episodes.ToDictionaryIgnoreDuplicates(v => v.Id);
                remoteEpisode.EpisodeRequested = remoteEpisode.Episodes.Any(v => requestedEpisodes.ContainsKey(v.Id));
            }

            return remoteEpisode;
        }

        public List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, bool sceneSource, SearchCriteriaBase searchCriteria = null)
        {
            if (sceneSource)
            {
                var remoteEpisode = Map(parsedEpisodeInfo, 0, 0, null, series, searchCriteria);

                return remoteEpisode.Episodes;
            }

            return GetEpisodes(parsedEpisodeInfo, series, parsedEpisodeInfo.SeasonNumber, sceneSource, searchCriteria);
        }

        private List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, int mappedSeasonNumber, bool sceneSource, SearchCriteriaBase searchCriteria)
        {
            if (parsedEpisodeInfo.FullSeason)
            {
                if (series.UseSceneNumbering && sceneSource)
                {
                    var episodes = episodeService.GetEpisodesBySceneSeason(series.Id, mappedSeasonNumber);

                    if (episodes.Any())
                    {
                        return episodes;
                    }
                }

                return episodeService.GetEpisodesBySeason(series.Id, mappedSeasonNumber);
            }

            if (parsedEpisodeInfo.IsDaily)
            {
                var episodeInfo = GetDailyEpisode(series, parsedEpisodeInfo.AirDate, parsedEpisodeInfo.DailyPart, searchCriteria);

                if (episodeInfo != null)
                {
                    return [episodeInfo];
                }

                return [];
            }

            if (parsedEpisodeInfo.IsAbsoluteNumbering)
            {
                return GetAnimeEpisodes(series, parsedEpisodeInfo, mappedSeasonNumber, sceneSource, searchCriteria);
            }

            if (parsedEpisodeInfo.IsPossibleSceneSeasonSpecial)
            {
                var parsedSpecialEpisodeInfo = ParseSpecialEpisodeTitle(parsedEpisodeInfo, parsedEpisodeInfo.ReleaseTitle, series);

                if (parsedSpecialEpisodeInfo != null)
                {
                    return GetStandardEpisodes(series, parsedSpecialEpisodeInfo, parsedSpecialEpisodeInfo.SeasonNumber, false, searchCriteria);
                }
            }

            if (parsedEpisodeInfo.Special && mappedSeasonNumber != 0)
            {
                return [];
            }

            return GetStandardEpisodes(series, parsedEpisodeInfo, mappedSeasonNumber, sceneSource, searchCriteria);
        }

        public ParsedEpisodeInfo ParseSpecialEpisodeTitle(ParsedEpisodeInfo parsedEpisodeInfo, string releaseTitle, int tvdbId, int tvRageId, string imdbId, SearchCriteriaBase searchCriteria = null)
        {
            if (searchCriteria != null)
            {
                if (tvdbId != 0 && tvdbId == searchCriteria.Series.TvdbId)
                {
                    return ParseSpecialEpisodeTitle(parsedEpisodeInfo, releaseTitle, searchCriteria.Series);
                }

                if (tvRageId != 0 && tvRageId == searchCriteria.Series.TvRageId)
                {
                    return ParseSpecialEpisodeTitle(parsedEpisodeInfo, releaseTitle, searchCriteria.Series);
                }

                if (imdbId.IsNotNullOrWhiteSpace() && imdbId.Equals(searchCriteria.Series.ImdbId, StringComparison.Ordinal))
                {
                    return ParseSpecialEpisodeTitle(parsedEpisodeInfo, releaseTitle, searchCriteria.Series);
                }
            }

            var series = GetSeries(releaseTitle);

            series ??= seriesService.FindByTitleInexact(releaseTitle);

            if (series == null && tvdbId > 0)
            {
                series = seriesService.FindByTvdbId(tvdbId);
            }

            if (series == null && tvRageId > 0)
            {
                series = seriesService.FindByTvRageId(tvRageId);
            }

            if (series == null && imdbId.IsNotNullOrWhiteSpace())
            {
                series = seriesService.FindByImdbId(imdbId);
            }

            if (series == null)
            {
                logger.Debug("No matching series {0}", releaseTitle);
                return null;
            }

            return ParseSpecialEpisodeTitle(parsedEpisodeInfo, releaseTitle, series);
        }

        public ParsedEpisodeInfo ParseSpecialEpisodeTitle(ParsedEpisodeInfo parsedEpisodeInfo, string releaseTitle, Series series)
        {
            // SxxE00 episodes are sometimes mapped via TheXEM, don't use episode title parsing in that case.
            if (parsedEpisodeInfo != null && parsedEpisodeInfo.IsPossibleSceneSeasonSpecial && series.UseSceneNumbering)
            {
                if (episodeService.FindEpisodesBySceneNumbering(series.Id, parsedEpisodeInfo.SeasonNumber, 0).Any())
                {
                    return parsedEpisodeInfo;
                }
            }

            // find special episode in series season 0
            var episode = episodeService.FindEpisodeByTitle(series.Id, 0, releaseTitle);

            if (episode != null)
            {
                // create parsed info from tv episode
                var info = new ParsedEpisodeInfo
                {
                    ReleaseTitle = releaseTitle,
                    SeriesTitle = series.Title,
                    SeriesTitleInfo = new SeriesTitleInfo
                    {
                        Title = series.Title
                    },
                    SeasonNumber = episode.SeasonNumber,
                    EpisodeNumbers = [episode.EpisodeNumber],
                    FullSeason = false,
                    Quality = QualityParser.ParseQuality(releaseTitle),
                    ReleaseGroup = ReleaseGroupParser.ParseReleaseGroup(releaseTitle),
                    Languages = LanguageParser.ParseLanguages(releaseTitle),
                    Special = true
                };

                logger.Debug("Found special episode {0} for title '{1}'", info, releaseTitle);
                return info;
            }

            return null;
        }

        private FindSeriesResult FindSeries(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, int tvRageId, string imdbId, SceneMapping sceneMapping, SearchCriteriaBase searchCriteria)
        {
            Series series = null;

            if (sceneMapping != null)
            {
                if (searchCriteria != null && searchCriteria.Series.TvdbId == sceneMapping.TvdbId)
                {
                    return new FindSeriesResult(searchCriteria.Series, SeriesMatchType.Alias);
                }

                series = seriesService.FindByTvdbId(sceneMapping.TvdbId);

                if (series == null)
                {
                    logger.Debug("No matching series {0}", parsedEpisodeInfo.SeriesTitle);
                    return null;
                }

                return new FindSeriesResult(series, SeriesMatchType.Alias);
            }

            if (searchCriteria != null)
            {
                if (searchCriteria.Series.CleanTitle == parsedEpisodeInfo.SeriesTitle.CleanSeriesTitle())
                {
                    return new FindSeriesResult(searchCriteria.Series, SeriesMatchType.Title);
                }

                if (tvdbId > 0 && tvdbId == searchCriteria.Series.TvdbId)
                {
                    logger.ForDebugEvent()
                           .Message("Found matching series by TVDB ID {0}, an alias may be needed for: {1}", tvdbId, parsedEpisodeInfo.SeriesTitle)
                           .Property("TvdbId", tvdbId)
                           .Property("ParsedEpisodeInfo", parsedEpisodeInfo)
                           .WriteSentryWarn("TvdbIdMatch", tvdbId.ToString(), parsedEpisodeInfo.SeriesTitle)
                           .Log();

                    return new FindSeriesResult(searchCriteria.Series, SeriesMatchType.Id);
                }

                if (tvRageId > 0 && tvRageId == searchCriteria.Series.TvRageId && tvdbId <= 0)
                {
                    logger.ForDebugEvent()
                           .Message("Found matching series by TVRage ID {0}, an alias may be needed for: {1}", tvRageId, parsedEpisodeInfo.SeriesTitle)
                           .Property("TvRageId", tvRageId)
                           .Property("ParsedEpisodeInfo", parsedEpisodeInfo)
                           .WriteSentryWarn("TvRageIdMatch", tvRageId.ToString(), parsedEpisodeInfo.SeriesTitle)
                           .Log();

                    return new FindSeriesResult(searchCriteria.Series, SeriesMatchType.Id);
                }

                if (imdbId.IsNotNullOrWhiteSpace() && imdbId.Equals(searchCriteria.Series.ImdbId, StringComparison.Ordinal) && tvdbId <= 0)
                {
                    logger.ForDebugEvent()
                           .Message("Found matching series by IMDb ID {0}, an alias may be needed for: {1}", imdbId, parsedEpisodeInfo.SeriesTitle)
                           .Property("ImdbId", imdbId)
                           .Property("ParsedEpisodeInfo", parsedEpisodeInfo)
                           .WriteSentryWarn("ImdbIdMatch", imdbId, parsedEpisodeInfo.SeriesTitle)
                           .Log();

                    return new FindSeriesResult(searchCriteria.Series, SeriesMatchType.Id);
                }
            }

            var matchType = SeriesMatchType.Unknown;
            series = seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitle);

            if (series != null)
            {
                matchType = SeriesMatchType.Title;
            }

            if (series == null && parsedEpisodeInfo.SeriesTitleInfo.AllTitles != null)
            {
                series = GetSeriesByAllTitles(parsedEpisodeInfo);
                matchType = SeriesMatchType.Title;
            }

            if (series == null && parsedEpisodeInfo.SeriesTitleInfo.Year > 0)
            {
                series = seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitleInfo.TitleWithoutYear, parsedEpisodeInfo.SeriesTitleInfo.Year);
                matchType = SeriesMatchType.Title;
            }

            if (series == null && tvdbId > 0)
            {
                series = seriesService.FindByTvdbId(tvdbId);

                if (series != null)
                {
                    logger.ForDebugEvent()
                           .Message("Found matching series by TVDB ID {0}, an alias may be needed for: {1}", tvdbId, parsedEpisodeInfo.SeriesTitle)
                           .Property("TvdbId", tvdbId)
                           .Property("ParsedEpisodeInfo", parsedEpisodeInfo)
                           .WriteSentryWarn("TvdbIdMatch", tvdbId.ToString(), parsedEpisodeInfo.SeriesTitle)
                           .Log();

                    matchType = SeriesMatchType.Id;
                }
            }

            if (series == null && tvRageId > 0 && tvdbId <= 0)
            {
                series = seriesService.FindByTvRageId(tvRageId);

                if (series != null)
                {
                    logger.ForDebugEvent()
                           .Message("Found matching series by TVRage ID {0}, an alias may be needed for: {1}", tvRageId, parsedEpisodeInfo.SeriesTitle)
                           .Property("TvRageId", tvRageId)
                           .Property("ParsedEpisodeInfo", parsedEpisodeInfo)
                           .WriteSentryWarn("TvRageIdMatch", tvRageId.ToString(), parsedEpisodeInfo.SeriesTitle)
                           .Log();

                    matchType = SeriesMatchType.Id;
                }
            }

            if (series == null && imdbId.IsNotNullOrWhiteSpace() && tvdbId <= 0)
            {
                series = seriesService.FindByImdbId(imdbId);

                if (series != null)
                {
                    logger.ForDebugEvent()
                           .Message("Found matching series by IMDb ID {0}, an alias may be needed for: {1}", imdbId, parsedEpisodeInfo.SeriesTitle)
                           .Property("ImdbId", imdbId)
                           .Property("ParsedEpisodeInfo", parsedEpisodeInfo)
                           .WriteSentryWarn("ImdbIdMatch", imdbId, parsedEpisodeInfo.SeriesTitle)
                           .Log();

                    matchType = SeriesMatchType.Id;
                }
            }

            // LLM FALLBACK MATCHING
            if (series == null && llmMatchingService.IsEnabled)
            {
                logger.Debug(
                    "All traditional matching methods failed for '{0}', attempting LLM matching",
                    parsedEpisodeInfo.SeriesTitle);

                try
                {
                    var availableSeries = seriesService.GetAllSeries();
                    var llmResult = llmMatchingService
                        .TryMatchSeriesAsync(parsedEpisodeInfo, availableSeries)
                        .GetAwaiter()
                        .GetResult();

                    if (llmResult?.IsSuccessfulMatch == true)
                    {
                        logger.Info(
                            "LLM matched '{0}' to '{1}' (TVDB: {2}) with {3:P0} confidence. Reasoning: {4}",
                            parsedEpisodeInfo.SeriesTitle,
                            llmResult.Series.Title,
                            llmResult.Series.TvdbId,
                            llmResult.Confidence,
                            llmResult.Reasoning);

                        return new FindSeriesResult(llmResult.Series, SeriesMatchType.Llm);
                    }

                    if (llmResult?.Alternatives?.Any() == true)
                    {
                        logger.Debug(
                            "LLM found potential but uncertain matches for '{0}': {1}",
                            parsedEpisodeInfo.SeriesTitle,
                            string.Join(", ", llmResult.Alternatives.Select(a => $"{a.Series.Title} ({a.Confidence:P0})")));
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(
                        ex,
                        "LLM matching failed for '{0}', falling back to manual import",
                        parsedEpisodeInfo.SeriesTitle);
                }
            }

            if (series == null)
            {
                logger.Debug("No matching series {0}", parsedEpisodeInfo.SeriesTitle);
                return null;
            }

            return new FindSeriesResult(series, matchType);
        }

        private Episode GetDailyEpisode(Series series, string airDate, int? part, SearchCriteriaBase searchCriteria)
        {
            Episode episodeInfo = null;

            if (searchCriteria != null)
            {
                episodeInfo = searchCriteria.Episodes.SingleOrDefault(
                    e => e.AirDate == airDate);
            }

            if (episodeInfo == null)
            {
                episodeInfo = episodeService.FindEpisode(series.Id, airDate, part);
            }

            return episodeInfo;
        }

        private List<Episode> GetAnimeEpisodes(Series series, ParsedEpisodeInfo parsedEpisodeInfo, int seasonNumber, bool sceneSource, SearchCriteriaBase searchCriteria)
        {
            var result = new List<Episode>();

            var sceneSeasonNumber = sceneMappingService.GetSceneSeasonNumber(parsedEpisodeInfo.SeriesTitle, parsedEpisodeInfo.ReleaseTitle);

            foreach (var absoluteEpisodeNumber in parsedEpisodeInfo.AbsoluteEpisodeNumbers)
            {
                var episodes = new List<Episode>();

                if (parsedEpisodeInfo.Special)
                {
                    var episode = episodeService.FindEpisode(series.Id, 0, absoluteEpisodeNumber);
                    episodes.AddIfNotNull(episode);
                }
                else if (sceneSource)
                {
                    if (sceneSeasonNumber.HasValue)
                    {
                        episodes = episodeService.FindEpisodesBySceneNumbering(series.Id, sceneSeasonNumber.Value, absoluteEpisodeNumber);

                        if (episodes.Empty())
                        {
                            var episode = episodeService.FindEpisode(series.Id, sceneSeasonNumber.Value, absoluteEpisodeNumber);
                            episodes.AddIfNotNull(episode);
                        }
                    }
                    else if (parsedEpisodeInfo.SeasonNumber > 1 && parsedEpisodeInfo.EpisodeNumbers.Empty())
                    {
                        episodes = episodeService.FindEpisodesBySceneNumbering(series.Id, parsedEpisodeInfo.SeasonNumber, absoluteEpisodeNumber);

                        if (episodes.Empty())
                        {
                            var episode = episodeService.FindEpisode(series.Id, parsedEpisodeInfo.SeasonNumber, absoluteEpisodeNumber);
                            episodes.AddIfNotNull(episode);
                        }
                    }
                    else
                    {
                        episodes = episodeService.FindEpisodesBySceneNumbering(series.Id, absoluteEpisodeNumber);

                        if (episodes.Count > 1)
                        {
                            episodes.Clear();
                        }
                    }
                }

                if (episodes.Empty())
                {
                    var episode = episodeService.FindEpisode(series.Id, absoluteEpisodeNumber);
                    episodes.AddIfNotNull(episode);
                }

                foreach (var episode in episodes)
                {
                    logger.Debug(
                        "Using absolute episode number {0} for: {1} - TVDB: {2}x{3:00}",
                        absoluteEpisodeNumber,
                        series.Title,
                        episode.SeasonNumber,
                        episode.EpisodeNumber);

                    result.Add(episode);
                }
            }

            return result;
        }

        private List<Episode> GetStandardEpisodes(Series series, ParsedEpisodeInfo parsedEpisodeInfo, int mappedSeasonNumber, bool sceneSource, SearchCriteriaBase searchCriteria)
        {
            var result = new List<Episode>();

            if (parsedEpisodeInfo.EpisodeNumbers == null)
            {
                return [];
            }

            foreach (var episodeNumber in parsedEpisodeInfo.EpisodeNumbers)
            {
                if (series.UseSceneNumbering && sceneSource)
                {
                    var episodes = new List<Episode>();

                    if (searchCriteria != null)
                    {
                        episodes = searchCriteria.Episodes.Where(e => e.SceneSeasonNumber == parsedEpisodeInfo.SeasonNumber &&
                                                                      e.SceneEpisodeNumber == episodeNumber).ToList();
                    }

                    if (!episodes.Any())
                    {
                        episodes = episodeService.FindEpisodesBySceneNumbering(series.Id, mappedSeasonNumber, episodeNumber);
                    }

                    if (episodes != null && episodes.Any())
                    {
                        logger.Debug(
                            "Using Scene to TVDB Mapping for: {0} - Scene: {1}x{2:00} - TVDB: {3}",
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
                    episodeInfo = searchCriteria.Episodes.SingleOrDefault(e => e.SeasonNumber == mappedSeasonNumber && e.EpisodeNumber == episodeNumber);
                }

                if (episodeInfo == null)
                {
                    episodeInfo = episodeService.FindEpisode(series.Id, mappedSeasonNumber, episodeNumber);
                }

                if (episodeInfo != null)
                {
                    result.Add(episodeInfo);
                }
                else
                {
                    logger.Debug("Unable to find {0}", parsedEpisodeInfo);
                }
            }

            return result;
        }
    }

    public class FindSeriesResult(Series series, SeriesMatchType matchType)
    {
        public Series Series { get; } = series;
        public SeriesMatchType MatchType { get; } = matchType;
    }
}
