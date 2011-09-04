using System;
using System.Collections.Generic;
using System.Text;

namespace NzbDrone.Core.Model
{
    public class MisnamedEpisodeModel
    {
        public int EpisodeFileId { get; set; }
        public int SeriesId { get; set; }
        public string SeriesTitle { get; set; }
        public string CurrentName { get; set; }
        public string ProperName { get; set; }
    }
}
