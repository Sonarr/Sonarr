using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.Tvdb.Resource
{
    public class TvdbResponseResource<T>
        where T : new()
    {
        public string Status { get; set; }
        public T Data { get; set; }
    }

    public class TvdbEpisodePageResource
    {
        public List<TvdbEpisodeResource> Episodes { get; set; }
        public TvdbSeriesStubResource Series { get; set; }

        public TvdbEpisodePageResource()
        {
            Episodes = new List<TvdbEpisodeResource>();
        }
    }

    public class TvdbSeriesStubResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TvdbLoginResource
    {
        public string ApiKey { get; set; }
    }

    public class TvdbTokenResource
    {
        public string Token { get; set; }
    }
}
