using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.Trakt;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Tv;
using Omu.ValueInjecter;
using RestSharp;
using Episode = NzbDrone.Core.Tv.Episode;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.MetadataSource
{
    public class TraktProxy : ISearchForNewSeries, IProvideSeriesInfo
    {
        private readonly Logger _logger;
        private static readonly Regex InvalidSearchCharRegex = new Regex(@"[^a-zA-Z0-9\s-\.]", RegexOptions.Compiled);

        public TraktProxy(Logger logger)
        {
            _logger = logger;
        }

        public List<Series> SearchForNewSeries(string title)
        {
            try
            {
                var client = BuildClient("search", "shows");
                var restRequest = new RestRequest(GetSearchTerm(title) + "/30/seasons");
                var response = client.ExecuteAndValidate<List<Show>>(restRequest);

                return response.Select(MapSeries).ToList();
            }
            catch (WebException ex)
            {
                throw new TraktException("Search for '{0}' failed. Unable to communicate with Trakt.", title);
            }
            catch (Exception ex)
            {
                _logger.WarnException(ex.Message, ex);
                throw new TraktException("Search for '{0}' failed. Invalid response received from Trakt.", title);
            }
        }

        public Tuple<Series, List<Episode>> GetSeriesInfo(int tvDbSeriesId)
        {
            var client = BuildClient("show", "summary");
            var restRequest = new RestRequest(tvDbSeriesId.ToString() + "/extended");
            var response = client.ExecuteAndValidate<Show>(restRequest);

            var episodes = response.seasons.SelectMany(c => c.episodes).Select(MapEpisode).ToList();
            var series = MapSeries(response);

            return new Tuple<Series, List<Episode>>(series, episodes);
        }

        private static IRestClient BuildClient(string resource, string method)
        {
            return new RestClient(string.Format("http://api.trakt.tv/{0}/{1}.json/bc3c2c460f22cbb01c264022b540e191", resource, method));
        }

        private static Series MapSeries(Show show)
        {
            var series = new Series();
            series.TvdbId = show.tvdb_id;
            series.TvRageId = show.tvrage_id;
            series.ImdbId = show.imdb_id;
            series.Title = show.title;
            series.CleanTitle = Parser.Parser.CleanSeriesTitle(show.title);
            series.Year = GetYear(show.year, show.first_aired);
            series.FirstAired = FromIso(show.first_aired_iso);
            series.Overview = show.overview;
            series.Runtime = show.runtime;
            series.Network = show.network;
            series.AirTime = show.air_time_utc;
            series.TitleSlug = show.url.ToLower().Replace("http://trakt.tv/show/", "");
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

        private static Episode MapEpisode(Trakt.Episode traktEpisode)
        {
            var episode = new Episode();
            episode.Overview = traktEpisode.overview;
            episode.SeasonNumber = traktEpisode.season;
            episode.EpisodeNumber = traktEpisode.episode;
            episode.EpisodeNumber = traktEpisode.number;
            episode.Title = traktEpisode.title;
            episode.AirDate = FromIsoToString(traktEpisode.first_aired_iso);
            episode.AirDateUtc = FromIso(traktEpisode.first_aired_iso);
            episode.Ratings = GetRatings(traktEpisode.ratings);

            episode.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Screenshot, traktEpisode.images.screen));

            return episode;
        }

        private static string GetPosterThumbnailUrl(string posterUrl)
        {
            if (posterUrl.Contains("poster-small.jpg")) return posterUrl;

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

        private static DateTime? FromEpoch(long ticks)
        {
            if (ticks == 0) return null;

            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds(ticks);
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
            phrase = phrase.RemoveAccent().ToLower();
            phrase = phrase.Replace("&", "and");
            phrase = InvalidSearchCharRegex.Replace(phrase, string.Empty);
            phrase = phrase.CleanSpaces().Replace(" ", "+");

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