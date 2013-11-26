using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{
    public class SampleResult
    {
        public string Filename { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
    }
}
