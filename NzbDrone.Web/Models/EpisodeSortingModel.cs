using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Model;

namespace NzbDrone.Web.Models
{
    public class EpisodeSortingModel
    {
        public bool ShowName { get; set; }
        public bool EpisodeName { get; set; }
        public bool ReplaceSpaces { get; set; }
        public bool AppendQuality { get; set; }
        public bool UseAirByDate { get; set; }
        public bool SeasonFolders { get; set; }
        public string SeasonFolderFormat { get; set; }
        public int SeparatorStyle { get; set; }
        public int NumberStyle { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public SelectList SeparatorStyles { get; set; }
        public SelectList NumberStyles { get; set; }
        public SelectList MultiEpisodeStyles { get; set; }
    }
}