using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.Trakt;
using NzbDrone.Core.Tv;
using RestSharp;
using NzbDrone.Common.EnsureThat;
using Episode = NzbDrone.Core.Tv.Episode;

namespace NzbDrone.Core.MetadataSource
{
    public class TraktProxy : ISearchForNewSeries, IProvideSeriesInfo, IProvideEpisodeInfo
    {
        public List<Series> SearchForNewSeries(string title)
        {
            var client = BuildClient("search", "shows");
            var restRequest = new RestRequest(title.ToSearchTerm());
            var response = client.Execute<List<Show>>(restRequest);

            return response.Data.Select(MapSeries).ToList();
        }

        public Series GetSeriesInfo(int tvDbSeriesId)
        {
            var client = BuildClient("show", "summary");
            var restRequest = new RestRequest(tvDbSeriesId.ToString());
            var response = client.Execute<Show>(restRequest);

            return MapSeries(response.Data);
        }

        public IList<Episode> GetEpisodeInfo(int tvDbSeriesId)
        {
            var client = BuildClient("show", "summary");
            var restRequest = new RestRequest(tvDbSeriesId.ToString() + "/extended");
            var response = client.Execute<Show>(restRequest);

            return response.Data.seasons.SelectMany(c => c.episodes).Select(MapEpisode).ToList();
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
            series.FirstAired = FromIso(show.first_aired_iso);
            series.Overview = show.overview;
            series.Runtime = show.runtime;
            series.Network = show.network;
            series.AirTime = show.air_time_utc;
            series.TitleSlug = show.url.ToLower().Replace("http://trakt.tv/show/", "");
            series.Status = GetSeriesStatus(show.status);

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
            episode.TvDbEpisodeId = traktEpisode.tvdb_id;
            episode.Title = traktEpisode.title;
            episode.AirDate = FromIso(traktEpisode.first_aired_iso);

            return episode;
        }

        private static string GetPosterThumbnailUrl(string posterUrl)
        {
            if (posterUrl.Contains("poster-small.jpg")) return posterUrl;

            var extension = Path.GetExtension(posterUrl);
            var withoutExtension = posterUrl.Substring(0, posterUrl.Length - extension.Length);
            return withoutExtension + "-300" + extension;
        }

        private static SeriesStatusType GetSeriesStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return SeriesStatusType.Continuing;
            if (status.Equals("Ended", StringComparison.InvariantCultureIgnoreCase)) return SeriesStatusType.Ended;
            return SeriesStatusType.Continuing;
        }

        private static DateTime? FromEpoch(long ticks)
        {
            if (ticks == 0) return null;

            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(ticks);
        }

        private static DateTime? FromIso(string iso)
        {
            DateTime result;

            if (!DateTime.TryParse(iso, out result))
                return null;

            return result.ToUniversalTime();
        }
    }
}