using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Model
{
    public class EpisodeRenameModel
    {
        public string SeriesName { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string EpisodeName { get; set; }
        public string Folder { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
    }
}
