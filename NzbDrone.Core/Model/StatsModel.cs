using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model
{
    public class StatsModel
    {
        [DisplayName("Number of Series")]
        public int SeriesTotal { get; set; }

        [DisplayName("Number of Series Countinuing")]
        public int SeriesContinuing { get; set; }

        [DisplayName("Number of Series Ended")]
        public int SeriesEnded { get; set; }

        [DisplayName("Number of Episodes")]
        public int EpisodesTotal { get; set; }

        [DisplayName("Number of Episodes On Disk")]
        public int EpisodesOnDisk { get; set; }

        [DisplayName("Number of Episodes Missing")]
        public int EpisodesMissing { get; set; }

        [DisplayName("Downloaded in the Last Week")]
        public int DownloadLastWeek { get; set; }

        [DisplayName("Downloaded in the Last 30 days")]
        public int DownloadedLastMonth { get; set; }
    }
}
