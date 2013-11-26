using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Episodes
{
    public class RenameEpisodeResource : RestResource
    {
        public Int32 SeriesId { get; set; }
        public Int32 SeasonNumber { get; set; }
        public List<Int32> EpisodeNumbers { get; set; }
        public Int32 EpisodeFileId { get; set; }
        public String ExistingPath { get; set; }
        public String NewPath { get; set; }
    }
}
