using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeResource
    {
        public Int32 SeriesId { get; set; }
        public Int32 EpisodeId { get; set; }
        public Int32 EpisodeFileId { get; set; }
        public Int32 SeasonNumber { get; set; }
        public Int32 EpisodeNumber { get; set; }
        public String Title { get; set; }
        public DateTime AirDate { get; set; }
        public Int32 Status { get; set; }
        public String Overview { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
    }
}
