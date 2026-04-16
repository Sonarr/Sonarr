using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DataAugmentation.DailySeries;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.MetadataSource.Tmdb.Resource;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource.Tmdb
{
    public class TmdbProxy : ITmdbMetadataSource
    {
        private const string ApiUrl = "https://api.themoviedb.org/3";
        private const string ImageBaseUrl = "https://image.tmdb.org/t/p/original";

        private readonly IHttpClient _httpClient;
        private readonly ISeriesService _seriesService;
        private readonly IDailySeriesService _dailySeriesService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public TmdbProxy(IHttpClient httpClient,
                         ISeriesService seriesService,
                         IDailySeriesService dailySeriesService,
                         IConfigService configService,
                         Logger logger)
        {
            _httpClient = httpClient;
            _seriesService = seriesService;
            _dailySeriesService = dailySeriesService;
            _configService = configService;
            _logger = logger;
        }

        public Tuple<Series, List<Episode>> GetSeriesInfo(int tvdbSeriesId, int tmdbSeriesId = 0)
        {
            var resolvedTmdbId = tmdbSeriesId > 0 ? tmdbSeriesId : FindTmdbIdByTvdbId(tvdbSeriesId);

            if (resolvedTmdbId <= 0)
            {
                throw new SeriesNotFoundException(tvdbSeriesId);
            }

            var details = Get<TmdbTvDetailsResource>($"tv/{resolvedTmdbId}", request =>
            {
                request.AddQueryParam("append_to_response", "external_ids,aggregate_credits,content_ratings,images");
                request.AddQueryParam("include_image_language", "en,null");
            });

            if (details.ExternalIds?.TvdbId is not > 0 && tvdbSeriesId > 0)
            {
                details.ExternalIds ??= new TmdbExternalIdsResource();
                details.ExternalIds.TvdbId = tvdbSeriesId;
            }

            var seasons = details.Seasons
                                 .Where(s => s.SeasonNumber >= 0)
                                 .Select(s => Get<TmdbSeasonDetailsResource>($"tv/{resolvedTmdbId}/season/{s.SeasonNumber}"))
                                 .ToList();

            var series = MapSeries(details);
            var runtime = details.EpisodeRunTime.FirstOrDefault();
            var episodes = seasons.SelectMany(s => s.Episodes.Select(e => MapEpisode(e, runtime))).ToList();

            return new Tuple<Series, List<Episode>>(series, episodes);
        }

        public List<Series> SearchForNewSeriesByImdbId(string imdbId)
        {
            imdbId = Parser.Parser.NormalizeImdbId(imdbId);

            if (imdbId == null)
            {
                return new List<Series>();
            }

            return FindByExternalId(imdbId, "imdb_id");
        }

        public List<Series> SearchForNewSeriesByAniListId(int aniListId)
        {
            return new List<Series>();
        }

        public List<Series> SearchForNewSeriesByTmdbId(int tmdbId)
        {
            if (tmdbId <= 0)
            {
                return new List<Series>();
            }

            try
            {
                return new List<Series> { GetSeriesInfo(0, tmdbId).Item1 }
                    .Where(s => s.TvdbId > 0)
                    .ToList();
            }
            catch (SeriesNotFoundException)
            {
                return new List<Series>();
            }
        }

        public List<Series> SearchForNewSeriesByMyAnimeListId(int malId)
        {
            return new List<Series>();
        }

        public List<Series> SearchForNewSeries(string title)
        {
            if (title.IsPathValid(PathValidationType.AnyOs))
            {
                throw new InvalidSearchTermException("Invalid search term '{0}'", title);
            }

            var lowerTitle = title.ToLowerInvariant().Trim();

            if (lowerTitle.StartsWith("tmdb:") || lowerTitle.StartsWith("tmdbid:"))
            {
                var slug = lowerTitle.Split(':')[1].Trim();

                if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out var tmdbId) || tmdbId <= 0)
                {
                    return new List<Series>();
                }

                return SearchForNewSeriesByTmdbId(tmdbId);
            }

            if (lowerTitle.StartsWith("tvdb:") || lowerTitle.StartsWith("tvdbid:"))
            {
                var slug = lowerTitle.Split(':')[1].Trim();

                if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out var tvdbId) || tvdbId <= 0)
                {
                    return new List<Series>();
                }

                return FindByExternalId(tvdbId.ToString(CultureInfo.InvariantCulture), "tvdb_id");
            }

            var response = Get<TmdbTvSearchResponse>("search/tv", request => request.AddQueryParam("query", title.Trim()));

            return response.Results
                           .Select(MapSearchResult)
                           .Where(s => s.TvdbId > 0)
                           .ToList();
        }

        private List<Series> FindByExternalId(string id, string externalSource)
        {
            var response = Get<TmdbFindResponse>($"find/{id}", request => request.AddQueryParam("external_source", externalSource));

            return response.TvResults
                           .Select(result => MapSearchResult(new TmdbTvSearchResult { Id = result.Id }))
                           .Where(series => series.TvdbId > 0)
                           .ToList();
        }

        private Series MapSearchResult(TmdbTvSearchResult searchResult)
        {
            var externalIds = Get<TmdbExternalIdsResource>($"tv/{searchResult.Id}/external_ids");
            var tvdbId = externalIds.TvdbId.GetValueOrDefault();

            if (tvdbId > 0)
            {
                var existingSeries = _seriesService.FindByTvdbId(tvdbId);

                if (existingSeries != null)
                {
                    return existingSeries;
                }
            }

            return MapSeries(searchResult, externalIds);
        }

        private Series MapSeries(TmdbTvSearchResult searchResult, TmdbExternalIdsResource externalIds)
        {
            var series = new Series
            {
                TvdbId = externalIds?.TvdbId ?? 0,
                TmdbId = searchResult.Id,
                ImdbId = externalIds?.ImdbId,
                Title = searchResult.Name,
                CleanTitle = Parser.Parser.CleanSeriesTitle(searchResult.Name),
                Overview = searchResult.Overview,
                Monitored = true,
                OriginalCountry = searchResult.OriginCountry.FirstOrDefault(),
                OriginalLanguage = searchResult.OriginalLanguage.IsNotNullOrWhiteSpace()
                    ? IsoLanguages.Find(searchResult.OriginalLanguage.ToLowerInvariant())?.Language ?? Language.English
                    : Language.English
            };

            series.SortTitle = SeriesTitleNormalizer.Normalize(series.Title, series.TvdbId > 0 ? series.TvdbId : series.TmdbId);
            series.TitleSlug = series.Title.CleanSeriesTitle();

            if (searchResult.FirstAirDate.IsNotNullOrWhiteSpace() && DateTime.TryParseExact(searchResult.FirstAirDate, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var firstAired))
            {
                series.FirstAired = firstAired;
                series.Year = firstAired.Year;
            }

            AddImage(series.Images, MediaCoverTypes.Poster, searchResult.PosterPath);
            AddImage(series.Images, MediaCoverTypes.Fanart, searchResult.BackdropPath);

            return series;
        }

        private Series MapSeries(TmdbTvDetailsResource details)
        {
            var series = new Series
            {
                TvdbId = details.ExternalIds?.TvdbId ?? 0,
                TmdbId = details.Id,
                ImdbId = details.ExternalIds?.ImdbId,
                Title = details.Name,
                CleanTitle = Parser.Parser.CleanSeriesTitle(details.Name),
                Overview = details.Overview,
                Monitored = true,
                Runtime = details.EpisodeRunTime.FirstOrDefault(),
                Network = details.Networks.FirstOrDefault()?.Name,
                Status = MapSeriesStatus(details.Status),
                Ratings = new Ratings { Votes = details.VoteCount, Value = details.VoteAverage },
                Genres = details.Genres.Select(g => g.Name).Where(g => g.IsNotNullOrWhiteSpace()).ToList(),
                OriginalCountry = details.OriginCountry.FirstOrDefault(),
                OriginalLanguage = details.OriginalLanguage.IsNotNullOrWhiteSpace()
                    ? IsoLanguages.Find(details.OriginalLanguage.ToLowerInvariant())?.Language ?? Language.English
                    : Language.English,
                Actors = details.AggregateCredits?.Cast.Select(MapActor).Where(a => a != null).ToList() ?? new List<Actor>(),
                Seasons = details.Seasons.Select(MapSeason).ToList(),
                TitleSlug = details.Name.CleanSeriesTitle(),
                Certification = MapCertification(details.ContentRatings)
            };

            series.SortTitle = SeriesTitleNormalizer.Normalize(series.Title, series.TvdbId > 0 ? series.TvdbId : series.TmdbId);

            if (details.FirstAirDate.IsNotNullOrWhiteSpace() && DateTime.TryParseExact(details.FirstAirDate, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var firstAired))
            {
                series.FirstAired = firstAired;
                series.Year = firstAired.Year;
            }

            if (details.LastAirDate.IsNotNullOrWhiteSpace() && DateTime.TryParseExact(details.LastAirDate, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var lastAired))
            {
                series.LastAired = lastAired;
            }

            AddImage(series.Images, MediaCoverTypes.Poster, details.PosterPath);
            AddImage(series.Images, MediaCoverTypes.Fanart, details.BackdropPath);

            foreach (var poster in details.Images?.Posters ?? new List<TmdbImageResource>())
            {
                AddImage(series.Images, MediaCoverTypes.Poster, poster.FilePath);
            }

            foreach (var backdrop in details.Images?.Backdrops ?? new List<TmdbImageResource>())
            {
                AddImage(series.Images, MediaCoverTypes.Fanart, backdrop.FilePath);
            }

            foreach (var logo in details.Images?.Logos ?? new List<TmdbImageResource>())
            {
                AddImage(series.Images, MediaCoverTypes.Clearlogo, logo.FilePath);
            }

            if (series.TvdbId > 0 && _dailySeriesService.IsDailySeries(series.TvdbId))
            {
                series.SeriesType = SeriesTypes.Daily;
            }

            return series;
        }

        private static Actor MapActor(TmdbCastResource cast)
        {
            if (cast?.Name.IsNullOrWhiteSpace() ?? true)
            {
                return null;
            }

            var actor = new Actor
            {
                Name = cast.Name,
                Character = cast.Roles.FirstOrDefault()?.Character
            };

            if (cast.ProfilePath.IsNotNullOrWhiteSpace())
            {
                actor.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Headshot, BuildImageUrl(cast.ProfilePath)));
            }

            return actor;
        }

        private static Season MapSeason(TmdbSeasonSummaryResource season)
        {
            var mapped = new Season
            {
                SeasonNumber = season.SeasonNumber,
                Monitored = season.SeasonNumber > 0
            };

            AddImage(mapped.Images, MediaCoverTypes.Poster, season.PosterPath);

            return mapped;
        }

        private static Episode MapEpisode(TmdbEpisodeResource episode, int seriesRuntime)
        {
            var mapped = new Episode
            {
                Overview = episode.Overview,
                SeasonNumber = episode.SeasonNumber,
                EpisodeNumber = episode.EpisodeNumber,
                Title = episode.Name,
                AirDate = episode.AirDate,
                Runtime = episode.Runtime ?? seriesRuntime,
                FinaleType = episode.EpisodeType,
                Ratings = new Ratings
                {
                    Votes = episode.VoteCount,
                    Value = episode.VoteAverage
                }
            };

            if (episode.AirDate.IsNotNullOrWhiteSpace() && DateTime.TryParseExact(episode.AirDate, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var airDateUtc))
            {
                mapped.AirDateUtc = airDateUtc;
            }

            AddImage(mapped.Images, MediaCoverTypes.Screenshot, episode.StillPath);

            return mapped;
        }

        private static string MapCertification(TmdbContentRatingsResource contentRatings)
        {
            var rating = contentRatings?.Results?.FirstOrDefault(r => r.CountryCode == "US" && r.Rating.IsNotNullOrWhiteSpace())
                         ?? contentRatings?.Results?.FirstOrDefault(r => r.CountryCode == "GB" && r.Rating.IsNotNullOrWhiteSpace())
                         ?? contentRatings?.Results?.FirstOrDefault(r => r.Rating.IsNotNullOrWhiteSpace());

            return rating?.Rating?.ToUpperInvariant();
        }

        private static SeriesStatusType MapSeriesStatus(string status)
        {
            if (status.IsNullOrWhiteSpace())
            {
                return SeriesStatusType.Continuing;
            }

            if (status.Equals("Ended", StringComparison.InvariantCultureIgnoreCase) ||
                status.Equals("Canceled", StringComparison.InvariantCultureIgnoreCase))
            {
                return SeriesStatusType.Ended;
            }

            if (status.Equals("Planned", StringComparison.InvariantCultureIgnoreCase) ||
                status.Equals("Pilot", StringComparison.InvariantCultureIgnoreCase) ||
                status.Equals("In Production", StringComparison.InvariantCultureIgnoreCase))
            {
                return SeriesStatusType.Upcoming;
            }

            return SeriesStatusType.Continuing;
        }

        private int FindTmdbIdByTvdbId(int tvdbId)
        {
            if (tvdbId <= 0)
            {
                return 0;
            }

            var response = Get<TmdbFindResponse>($"find/{tvdbId}", request => request.AddQueryParam("external_source", "tvdb_id"));

            return response.TvResults.FirstOrDefault()?.Id ?? 0;
        }

        private T Get<T>(string resource, Action<HttpRequestBuilder> builderAction = null)
            where T : new()
        {
            EnsureApiKey();

            var requestBuilder = new HttpRequestBuilder(ApiUrl)
                .Resource(resource)
                .AddQueryParam("api_key", _configService.TmdbApiKey);

            builderAction?.Invoke(requestBuilder);

            var request = requestBuilder.Build();
            request.SuppressHttpError = true;

            var response = _httpClient.Get<T>(request);

            if (response.HasHttpError)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new SeriesNotFoundException(0, $"TMDb resource '{resource}' was not found.");
                }

                _logger.Warn("TMDb request failed for resource {0} with status {1}", resource, response.StatusCode);
                throw new HttpException(request, response);
            }

            return response.Resource;
        }

        private void EnsureApiKey()
        {
            if (_configService.TmdbApiKey.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("TMDb API key is not configured.");
            }
        }

        private static void AddImage(ICollection<MediaCover.MediaCover> images, MediaCoverTypes coverType, string path)
        {
            if (path.IsNullOrWhiteSpace())
            {
                return;
            }

            var imageUrl = BuildImageUrl(path);

            if (images.Any(i => i.CoverType == coverType && i.RemoteUrl == imageUrl))
            {
                return;
            }

            images.Add(new MediaCover.MediaCover(coverType, imageUrl));
        }

        private static string BuildImageUrl(string path)
        {
            if (path.IsNullOrWhiteSpace())
            {
                return null;
            }

            if (path.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                return path;
            }

            return $"{ImageBaseUrl}{path}";
        }
    }
}
