using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.Tvdb.Resource
{
    public class TvdbSeriesExtendedResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TvdbStatusResource Status { get; set; }
        public List<TvdbSeasonResource> Seasons { get; set; }
    }

    public class TvdbStatusResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TvdbSeasonResource
    {
        public int Id { get; set; }
        public int? Number { get; set; }
        public TvdbSeasonTypeResource Type { get; set; }
    }

    public class TvdbSeasonTypeResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
