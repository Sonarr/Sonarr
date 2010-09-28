using System;
using System.ServiceModel.Syndication;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Season
    {
        [SubSonicPrimaryKey]
        public string SeasonId { get; set; }
        public long SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public bool Monitored { get; set; }
        public string Folder { get; set; }
    }
}