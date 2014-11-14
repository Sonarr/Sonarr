using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.Trakt;
using NzbDrone.Core.Tv;
using Episode = NzbDrone.Core.Tv.Episode;

namespace NzbDrone.Core.MetadataSource
{
    public class TraktProxy : ISearchForNewSeries, IProvideSeriesInfo
    {
        private readonly Logger _logger;
        private readonly IHttpClient _httpClient;
        private static readonly Regex CollapseSpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex InvalidSearchCharRegex = new Regex(@"(?:\*|\(|\)|'|!|@|\+)", RegexOptions.Compiled);
        private static readonly Regex ExpandCamelCaseRegEx = new Regex(@"(?<!^|[A-Z]\.?|[^\w.])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|(?<!^|\d\.?|[^\w.])(?=\d)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private readonly HttpRequestBuilder _requestBuilder;

        public TraktProxy(Logger logger, IHttpClient httpClient)
        {
            _requestBuilder = new HttpRequestBuilder("http://api.trakt.tv/{path}/{resource}.json/bc3c2c460f22cbb01c264022b540e191");
            _logger = logger;
            _httpClient = httpClient;
        }

        private IEnumerable<Show> SearchTrakt(string title)
        {
            HttpRequest request;

            var lowerTitle = title.ToLowerInvariant();

            if (lowerTitle.StartsWith("tvdb:") || lowerTitle.StartsWith("tvdbid:") || lowerTitle.StartsWith("slug:"))
            {
                var slug = lowerTitle.Split(':')[1].Trim();

                if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace))
                {
                    return Enumerable.Empty<Show>();
                }

                request = _requestBuilder.Build("/{slug}/extended");

                request.AddSegment("path", "show");
                request.AddSegment("resource", "summary");
                request.AddSegment("slug", GetSearchTerm(slug));

                return new List<Show> { _httpClient.Get<Show>(request).Resource };
            }

            if (lowerTitle.StartsWith("imdb:") || lowerTitle.StartsWith("imdbid:"))
            {
                var slug = lowerTitle.Split(':')[1].TrimStart('t').Trim();

                if (slug.IsNullOrWhiteSpace() || !slug.All(char.IsDigit) || slug.Length < 7)
                {
                    return Enumerable.Empty<Show>();
                }

                title = "tt" + slug;
            }

            request = _requestBuilder.Build("");

            request.AddSegment("path", "search");
            request.AddSegment("resource", "shows");
            request.UriBuilder.SetQueryParam("query", GetSearchTerm(title));
            request.UriBuilder.SetQueryParam("seasons", true);

            return _httpClient.Get<List<Show>>(request).Resource;
        }

        public List<Series> SearchForNewSeries(string title)
        {
            try
            {
                var series = SearchTrakt(title.Trim()).Select(MapSeries).ToList();

                series.Sort(new TraktSearchSeriesComparer(title));

                return series;
            }
            catch (HttpException)
            {
                throw new TraktException("Search for '{0}' failed. Unable to communicate with Trakt.", title);
            }
            catch (Exception ex)
            {
                _logger.WarnException(ex.Message, ex);
                throw new TraktException("Search for '{0}' failed. Invalid response received from Trakt.", title);
            }
        }

        public Tuple<Series, List<Episode>> GetSeriesInfo(int tvdbSeriesId)
        {

            var request = _requestBuilder.Build("/{tvdbId}/extended");

            request.AddSegment("path", "show");
            request.AddSegment("resource", "summary");
            request.AddSegment("tvdbId", tvdbSeriesId.ToString());

            var response = _httpClient.Get<Show>(request).Resource;

            var episodes = response.seasons.SelectMany(c => c.episodes).Select(MapEpisode);

            episodes = RemoveDuplicates(episodes);
               
            var series = MapSeries(response);

            return new Tuple<Series, List<Episode>>(series, episodes.ToList());
        }

        private static IEnumerable<Episode> RemoveDuplicates(IEnumerable<Episode> episodes)
        {
            //http://support.trakt.tv/forums/188762-general/suggestions/4430690-anger-management-duplicate-episode
            var episodeGroup = episodes.GroupBy(e => e.SeasonNumber.ToString("0000") + e.EpisodeNumber.ToString("0000"));
            return episodeGroup.Select(g => g.First());
        }

        private static Series MapSeries(Show show)
        {
            var series = new Series();
            series.TvdbId = show.tvdb_id;
            series.TvRageId = show.tvrage_id;
            series.ImdbId = show.imdb_id;
            series.Title = show.title;
            series.CleanTitle = Parser.Parser.CleanSeriesTitle(show.title);
            series.SortTitle = SeriesTitleNormalizer.Normalize(show.title, show.tvdb_id);
            series.Year = GetYear(show.year, show.first_aired);
            series.FirstAired = FromIso(show.first_aired_iso);
            series.Overview = show.overview;
            series.Runtime = show.runtime;
            series.Network = show.network;
            series.AirTime = show.air_time;
            series.TitleSlug = GetTitleSlug(show.url);
            series.Status = GetSeriesStatus(show.status, show.ended);
            series.Ratings = GetRatings(show.ratings);
            series.Genres = show.genres;
            series.Certification = show.certification;
            series.Actors = GetActors(show.people);
            series.Seasons = GetSeasons(show);

            series.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Banner, Url = show.images.banner });
            series.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Poster, Url = GetPosterThumbnailUrl(show.images.poster) });
            series.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Fanart, Url = show.images.fanart });

            return series;
        }

        private static String GetTitleSlug(String url)
        {
            var slug = url.ToLower().Replace("http://trakt.tv/show/", "");

            if (slug.StartsWith("."))
            {
                slug = "dot" + slug.Substring(1);
            }

            return slug;
        }

        private static Episode MapEpisode(Trakt.Episode traktEpisode)
        {
            var episode = new Episode();
            episode.Overview = traktEpisode.overview;
            episode.SeasonNumber = traktEpisode.season;
            episode.EpisodeNumber = traktEpisode.number;
            episode.Title = traktEpisode.title;
            episode.AirDate = FromIsoToString(traktEpisode.first_aired_iso);
            episode.AirDateUtc = FromIso(traktEpisode.first_aired_iso);
            episode.Ratings = GetRatings(traktEpisode.ratings);

            //Don't include series fanart images as episode screenshot
            if (!traktEpisode.images.screen.Contains("-940."))
            {
                episode.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Screenshot, traktEpisode.images.screen));
            }
            
            return episode;
        }

        private static string GetPosterThumbnailUrl(string posterUrl)
        {
            if (posterUrl.Contains("poster-small.jpg") || posterUrl.Contains("poster-dark.jpg")) return posterUrl;

            var extension = Path.GetExtension(posterUrl);
            var withoutExtension = posterUrl.Substring(0, posterUrl.Length - extension.Length);
            return withoutExtension + "-300" + extension;
        }

        private static SeriesStatusType GetSeriesStatus(string status, bool? ended)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                if (ended.HasValue && ended.Value)
                {
                    return SeriesStatusType.Ended;
                }

                return SeriesStatusType.Continuing;
            }
            if (status.Equals("Ended", StringComparison.InvariantCultureIgnoreCase)) return SeriesStatusType.Ended;
            return SeriesStatusType.Continuing;
        }

        private static DateTime? FromIso(string iso)
        {
            DateTime result;

            if (!DateTime.TryParse(iso, out result))
                return null;

            return result.ToUniversalTime();
        }

        private static string FromIsoToString(string iso)
        {
            if (String.IsNullOrWhiteSpace(iso)) return null;

            var match = Regex.Match(iso, @"^\d{4}\W\d{2}\W\d{2}");

            if (!match.Success) return null;

            return match.Captures[0].Value;
        }

        private static string GetSearchTerm(string phrase)
        {
            phrase = phrase.RemoveAccent();
            phrase = InvalidSearchCharRegex.Replace(phrase, "");

            if (!phrase.Any(char.IsWhiteSpace) && phrase.Any(char.IsUpper) && phrase.Any(char.IsLower) && phrase.Length > 4)
            {
                phrase = ExpandCamelCaseRegEx.Replace(phrase, " ");
            }

            phrase = CollapseSpaceRegex.Replace(phrase, " ").Trim();
            phrase = phrase.Trim('-');

            phrase = System.Web.HttpUtility.UrlEncode(phrase.ToLower());

            return phrase;
        }

        private static int GetYear(int year, int firstAired)
        {
            if (year > 1969) return year;

            if (firstAired == 0) return DateTime.Today.Year;

            return year;
        }

        private static Tv.Ratings GetRatings(Trakt.Ratings ratings)
        {
            return new Tv.Ratings
                   {
                       Percentage = ratings.percentage,
                       Votes = ratings.votes,
                       Loved = ratings.loved,
                       Hated = ratings.hated
                   };
        }

        private static List<Tv.Actor> GetActors(People people)
        {
            if (people == null)
            {
                return new List<Tv.Actor>();
            }

            return GetActors(people.actors).ToList();
        }

        private static IEnumerable<Tv.Actor> GetActors(IEnumerable<Trakt.Actor> trakcActors)
        {
            foreach (var traktActor in trakcActors)
            {
                var actor = new Tv.Actor
                            {
                                Name = traktActor.name,
                                Character = traktActor.character,
                            };

                actor.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Headshot, traktActor.images.headshot));

                yield return actor;
            }
        }

        private static List<Tv.Season> GetSeasons(Show show)
        {
            var seasons = new List<Tv.Season>();

            foreach (var traktSeason in show.seasons.OrderByDescending(s => s.season))
            {
                var season = new Tv.Season
                {
                    SeasonNumber = traktSeason.season
                };

                if (traktSeason.images != null)
                {
                    season.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Poster, traktSeason.images.poster));
                }

                seasons.Add(season);
            }

            return seasons;
        }
    }
}
