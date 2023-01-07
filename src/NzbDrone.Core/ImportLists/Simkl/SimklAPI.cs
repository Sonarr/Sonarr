using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.Simkl
{
    public class SimklSeriesIdsResource
    {
        public int Simkl { get; set; }
        public string Slug { get; set; }
        public string Imdb { get; set; }
        public string Tmdb { get; set; }
        public string Tvdb { get; set; }
    }

    public class SimklSeriesPropsResource
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public SimklSeriesIdsResource Ids { get; set; }
    }

    public class SimklSeriesResource
    {
        public SimklSeriesPropsResource Show { get; set; }
    }

    public class SimklResponse
    {
        public List<SimklSeriesResource> Shows { get; set; }
    }

    public class RefreshRequestResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class UserSettingsResponse
    {
        public SimklUserResource User { get; set; }
        public SimklUserAccountResource Account { get; set; }
    }

    public class SimklUserResource
    {
        public string Name { get; set; }
    }

    public class SimklUserAccountResource
    {
        public int Id { get; set; }
    }

    public class SimklSyncActivityResource
    {
        [JsonProperty("tv_shows")]
        public SimklTvSyncActivityResource TvShows { get; set; }
    }

    public class SimklTvSyncActivityResource
    {
        public DateTime All { get; set; }
    }
}
