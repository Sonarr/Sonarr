using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NzbDrone.Web.Models
{
    public class EpisodeSortingModel
    {
        [DisplayName("Show Name")]
        public bool ShowName { get; set; }

        [DisplayName("Episode Name")]
        public bool EpisodeName { get; set; }

        [DisplayName("Replace Spaces")]
        public bool ReplaceSpaces { get; set; }

        [DisplayName("Append Quality")]
        public bool AppendQuality { get; set; }

        [DisplayName("Use Air By Date")]
        public bool UseAirByDate { get; set; }

        [DisplayName("Use Season Folders")]
        public bool SeasonFolders { get; set; }

        [DisplayName("Season Folder Format")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Description = "This is a Description")]
        public string SeasonFolderFormat { get; set; }

        [DisplayName("Separator Style")]
        public int SeparatorStyle { get; set; }

        [DisplayName("Numbering Style")]
        public int NumberStyle { get; set; }

        [DisplayName("Multi-Episode Style")]
        public int MultiEpisodeStyle { get; set; }

        public SelectList SeparatorStyles { get; set; }
        public SelectList NumberStyles { get; set; }
        public SelectList MultiEpisodeStyles { get; set; }
    }
}