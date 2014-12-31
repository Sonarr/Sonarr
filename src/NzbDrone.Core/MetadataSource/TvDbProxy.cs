using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.Trakt;
using NzbDrone.Core.Tv;
using TVDBSharp;
using TVDBSharp.Models.Enums;

namespace NzbDrone.Core.MetadataSource
{
    public class TvDbProxy : ISearchForNewSeries, IProvideSeriesInfo
    {
        private readonly Logger _logger;
        private static readonly Regex CollapseSpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex InvalidSearchCharRegex = new Regex(@"(?:\*|\(|\)|'|!|@|\+)", RegexOptions.Compiled);

        private readonly TVDB _tvdb;

        public TvDbProxy(Logger logger)
        {
            _logger = logger;
            _tvdb = new TVDB("5D2D188E86E07F4F");
        }

        private IEnumerable<TVDBSharp.Models.Show> SearchTrakt(string title)
        {
            var lowerTitle = title.ToLowerInvariant();

            if (lowerTitle.StartsWith("tvdb:") || lowerTitle.StartsWith("tvdbid:"))
            {
                var slug = lowerTitle.Split(':')[1].Trim();

                int tvdbId;

                if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !Int32.TryParse(slug, out tvdbId))
                {
                    return Enumerable.Empty<TVDBSharp.Models.Show>();
                }

                return new[] { _tvdb.GetShow(tvdbId) };
            }

            return _tvdb.Search(GetSearchTerm(lowerTitle), 10);
        }

        public List<Series> SearchForNewSeries(string title)
        {
            try
            {
                var tvdbSeries = SearchTrakt(title.Trim());

                var series = tvdbSeries.Select(MapSeries).ToList();

                series.Sort(new TraktSearchSeriesComparer(title));

                return series;
            }
            catch (Common.Http.HttpException)
            {
                throw new TraktException("Search for '{0}' failed. Unable to communicate with Trakt.", title);
            }
            catch (Exception ex)
            {
                _logger.WarnException(ex.Message, ex);
                throw new TraktException("Search for '{0}' failed. Invalid response received from Trakt.", title);
            }
        }

        public Tuple<Series, List<Tv.Episode>> GetSeriesInfo(int tvdbSeriesId)
        {
            var tvdbSeries = _tvdb.GetShow(tvdbSeriesId);

            var episodes = tvdbSeries.Episodes.Select(MapEpisode);

            episodes = RemoveDuplicates(episodes);

            var series = MapSeries(tvdbSeries);

            return new Tuple<Series, List<Tv.Episode>>(series, episodes.ToList());
        }

        private static IEnumerable<Tv.Episode> RemoveDuplicates(IEnumerable<Tv.Episode> episodes)
        {
            //http://support.trakt.tv/forums/188762-general/suggestions/4430690-anger-management-duplicate-episode
            var episodeGroup = episodes.GroupBy(e => e.SeasonNumber.ToString("0000") + e.EpisodeNumber.ToString("0000"));
            return episodeGroup.Select(g => g.First());
        }

        private static Series MapSeries(TVDBSharp.Models.Show show)
        {
            var series = new Series();
            series.TvdbId = show.Id;
            //series.TvRageId = show.tvrage_id;
            series.ImdbId = show.ImdbId;
            series.Title = show.Name;
            series.CleanTitle = Parser.Parser.CleanSeriesTitle(show.Name);
            series.SortTitle = SeriesTitleNormalizer.Normalize(show.Name, show.Id);

            if (show.FirstAired != null)
            {
                series.Year = show.FirstAired.Value.Year;
                series.FirstAired = show.FirstAired.Value.ToUniversalTime();
            }

            series.Overview = show.Description;

            if (show.Runtime != null)
            {
                series.Runtime = show.Runtime.Value;
            }

            series.Network = show.Network;

            if (show.AirTime != null)
            {
                series.AirTime = show.AirTime.Value.ToString();
            }

            series.TitleSlug = GenerateSlug(show.Name);
            series.Status = GetSeriesStatus(show.Status);
            series.Ratings = GetRatings(show.RatingCount, show.Rating);
            series.Genres = show.Genres;
            series.Certification = show.ContentRating.ToString().ToUpper();
            series.Actors = new List<Tv.Actor>();
            series.Seasons = GetSeasons(show);

            series.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Banner, Url = show.Banner.ToString() });
            series.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Poster, Url = show.Poster.ToString() });
            series.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Fanart, Url = show.Fanart.ToString() });

            return series;
        }

        private static Tv.Episode MapEpisode(TVDBSharp.Models.Episode traktEpisode)
        {
            var episode = new Tv.Episode();
            episode.Overview = traktEpisode.Description;
            episode.SeasonNumber = traktEpisode.SeasonNumber;
            episode.EpisodeNumber = traktEpisode.EpisodeNumber;
            episode.Title = traktEpisode.Title;

            if (traktEpisode.FirstAired != null)
            {
                episode.AirDate = traktEpisode.FirstAired.Value.ToString("yyyy-MM-dd");
                episode.AirDateUtc = traktEpisode.FirstAired.Value.ToUniversalTime();
            }

            episode.Ratings = GetRatings(traktEpisode.RatingCount, traktEpisode.Rating);

            //Don't include series fanart images as episode screenshot
            episode.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Screenshot, traktEpisode.EpisodeImage.ToString()));

            return episode;
        }


        private static SeriesStatusType GetSeriesStatus(Status status)
        {
            if (status == Status.Ended)
            {
                return SeriesStatusType.Ended;
            }

            return SeriesStatusType.Continuing;
        }

        private static string GetSearchTerm(string phrase)
        {
            phrase = phrase.RemoveAccent();
            phrase = InvalidSearchCharRegex.Replace(phrase, "");

            //            if (!phrase.Any(char.IsWhiteSpace) && phrase.Any(char.IsUpper) && phrase.Any(char.IsLower) && phrase.Length > 4)
            //            {
            //                phrase = ExpandCamelCaseRegEx.Replace(phrase, " ");
            //            }

            phrase = CollapseSpaceRegex.Replace(phrase, " ").Trim();
            phrase = phrase.Trim('-');

            phrase = HttpUtility.UrlEncode(phrase.ToLower());

            return phrase;
        }

        private static Tv.Ratings GetRatings(int ratingCount, double? rating)
        {

            var result = new Tv.Ratings { Votes = ratingCount };

            if (rating != null)
            {
                result.Percentage = (int)(rating.Value * 100);
            }

            return result;
        }


        private static List<Tv.Season> GetSeasons(TVDBSharp.Models.Show show)
        {
            var seasons = new List<Tv.Season>();

            foreach (var seasonNumber in show.Episodes.Select(c => c.SeasonNumber).Distinct().OrderByDescending(c => c))
            {
                var season = new Tv.Season
                {
                    SeasonNumber = seasonNumber
                };

                /*           if (season.images != null)
                           {
                               season.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Poster, season.images.poster));
                           }*/

                seasons.Add(season);
            }

            return seasons;
        }


        private static readonly Regex RemoveRegex = new Regex(@"[^\w-]", RegexOptions.Compiled);

        public static string GenerateSlug(string showTitle)
        {
            if (showTitle.StartsWith("."))
            {
                showTitle = "dot" + showTitle.Substring(1);
            }
            showTitle = showTitle.Replace(" ", "-");
            showTitle = showTitle.Replace("&", "and");
            showTitle = RemoveRegex.Replace(showTitle, String.Empty);
            showTitle = showTitle.RemoveAccent();
            showTitle = showTitle.ToLowerInvariant();

            return showTitle;
        }
    }
}
