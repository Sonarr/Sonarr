using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RestSharp;

namespace NzbDrone.Core.Tvdb
{
    public partial class Tvdb
    {
        private const string BASE_URL = "http://www.thetvdb.com/api";

        public string ApiKey { get; set; }
        public string Error { get; set; }

        /// <summary>
        ///     String representation of response content
        /// </summary>
        public string ResponseContent { get; set; }

        /// <summary>
        ///     Dictionary of Header values in response
        ///     http://help.themoviedb.org/kb/api/content-versioning
        /// </summary>
        public Dictionary<string, object> ResponseHeaders { get; set; }

#if !WINDOWS_PHONE
        /// <summary>
        ///     Proxy to use for requests made.  Passed on to underying WebRequest if set.
        /// </summary>
        public IWebProxy Proxy { get; set; }
#endif

        /// <summary>
        ///     Timeout in milliseconds to use for requests made.
        /// </summary>
        public int? Timeout { get; set; }

        public Tvdb(string apiKey)
        {
            ApiKey = apiKey;
            Error = null;
            Timeout = null;
        }

        #region Helper methods

        public static string GetImageUrl(string BannerMirror, string filename)
        {
            return string.Format("{0}/banners/{1}", BannerMirror, filename);
        }

#if !WINDOWS_PHONE
        public static byte[] GetImage(string BannerMirror, string filename)
        {
            return GetImage(GetImageUrl(BannerMirror, filename));
        }

        public static byte[] GetImage(string url)
        {
            return new WebClient().DownloadData(url);
        }
#endif

        #endregion

        #region Build Requests

        private RestRequest BuildGetMirrorsRequest(object UserState = null)
        {
            var request = new RestRequest("{apikey}/mirrors.xml", Method.GET);
            request.AddUrlSegment("apikey", ApiKey);
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private static RestRequest BuildGetServerTimeRequest(object UserState = null)
        {
            var request = new RestRequest("Updates.php", Method.GET);
            request.AddParameter("type", "none");
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private RestRequest BuildGetLanguagesRequest(object UserState = null)
        {
            var request = new RestRequest("{apikey}/languages.xml", Method.GET);
            request.AddUrlSegment("apikey", ApiKey);
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private static RestRequest BuildGetSearchSeriesRequest(string search, object UserState = null)
        {
            var request = new RestRequest("GetSeries.php", Method.GET);
            request.AddParameter("seriesname", search);
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private RestRequest BuildGetSeriesBaseRecordRequest(int SeriesId, string Language, object UserState = null)
        {
            var request = new RestRequest("api/{apikey}/series/{id}/{lang}.xml");
            request.AddUrlSegment("apikey", ApiKey);
            request.AddUrlSegment("id", SeriesId.ToString());
            request.AddUrlSegment("lang", Language);
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private RestRequest BuildGetSeriesFullRecordRequest(int SeriesId, string Language, object UserState = null)
        {
            var request = new RestRequest("api/{apikey}/series/{id}/all/{lang}.xml");
            request.AddUrlSegment("apikey", ApiKey);
            request.AddUrlSegment("id", SeriesId.ToString());
            request.AddUrlSegment("lang", Language);
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private RestRequest BuildGetSeriesBannersRequest(int SeriesId, object UserState = null)
        {
            var request = new RestRequest("api/{apikey}/series/{id}/banners.xml");
            request.AddUrlSegment("apikey", ApiKey);
            request.AddUrlSegment("id", SeriesId.ToString());
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private RestRequest BuildGetSeriesActorsRequest(int SeriesId, object UserState = null)
        {
            var request = new RestRequest("api/{apikey}/series/{id}/actors.xml");
            request.AddUrlSegment("apikey", ApiKey);
            request.AddUrlSegment("id", SeriesId.ToString());
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private RestRequest BuildGetEpisodeRequest(int EpisodeId, string Language, object UserState = null)
        {
            var request = new RestRequest("api/{apikey}/episodes/{id}/{lang}.xml");
            request.AddUrlSegment("apikey", ApiKey);
            request.AddUrlSegment("id", EpisodeId.ToString());
            request.AddUrlSegment("lang", Language);
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private RestRequest BuildGetSeriesEpisodeRequest(int SeriesId, int SeasonNum, int EpisodeNum, string Language,
                                                         object UserState = null)
        {
            var request = new RestRequest("api/{apikey}/series/{id}/default/{season}/{episode}/{lang}.xml");
            request.AddUrlSegment("apikey", ApiKey);
            request.AddUrlSegment("id", SeriesId.ToString());
            request.AddUrlSegment("season", SeasonNum.ToString());
            request.AddUrlSegment("episode", EpisodeNum.ToString());
            request.AddUrlSegment("lang", Language);
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private RestRequest BuildGetUpdatesRequest(TvdbUpdatePeriod Period, object UserState = null)
        {
            var request = new RestRequest("api/{apikey}/updates/updates_{period}.xml");
            request.AddUrlSegment("apikey", ApiKey);
            request.AddUrlSegment("period", Period.ToString());
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        private static RestRequest BuildGetUpdatesSinceRequest(Int64 LastTime, object UserState = null)
        {
            var request = new RestRequest("api/Updates.php?type=all&time={time}");
            request.AddUrlSegment("time", LastTime.ToString());
            if(UserState != null)
                request.UserState = UserState;

            return request;
        }

        #endregion
    }
}
