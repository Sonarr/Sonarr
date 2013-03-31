using System.Collections.Generic;
using System.Linq;
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
            var restRequest = new RestRequest(title.ToSlug().Replace("-", "+"));
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
            return new RestClient(string.Format("http://api.trakt.tv/{0}/{1}.json/6fc98f83c6a02decd17eb7e13d00e89c", resource, method));
        }

        private static Series MapSeries(Show show)
        {
            var series = new Series();
            series.TvDbId = show.tvdb_id;
            series.TvRageId = show.tvrage_id;
            series.ImdbId = show.imdb_id;
            series.Title = show.title;
            series.FirstAired = show.first_aired;
            series.Overview = show.overview;
            series.Runtime = show.runtime;
            series.Network = show.network;
            series.AirTime = show.air_time;
            series.TitleSlug = show.url.ToLower().Replace("http://trakt.tv/show/", "");

            series.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Banner, Url = show.images.banner });
            series.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Poster, Url = show.images.poster });
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
            episode.AirDate = traktEpisode.first_aired;

            return episode;
        }


    }
}