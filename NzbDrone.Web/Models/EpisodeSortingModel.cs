using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NzbDrone.Web.Models
{
    public class EpisodeSortingModel
    {
        [DisplayName("Series Name")]
        [Description("Should filenames contain the series name when renamed?")]
        public bool SeriesName { get; set; }

        [DisplayName("Episode Name")]
        [Description("Should filenames contain the episode name when renamed?")]
        public bool EpisodeName { get; set; }

        [DisplayName("Replace Spaces")]
        [Description("Do you want to replace spaces in the filename with periods?")]
        public bool ReplaceSpaces { get; set; }

        [DisplayName("Append Quality")]
        [Description("Should filenames have the quality appended to the end?")]
        public bool AppendQuality { get; set; }

        [DisplayName("Use Season Folders")]
        [Description("Should files be stored in season folders?")]
        public bool SeasonFolders { get; set; }

        [DisplayName("Season Folder Format")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Description("How should season folders be named? (Use %0s to pad to two digits)")]
        public string SeasonFolderFormat { get; set; }

        [DisplayName("Separator Style")]
        [Description("How should NzbDrone separate sections of the filename?")]
        public int SeparatorStyle { get; set; }

        [DisplayName("Numbering Style")]
        [Description("What numbering style do you want?")]
        public int NumberStyle { get; set; }

        [DisplayName("Multi-Episode Style")]
        [Description("How will multi-episode files be named?")]
        public int MultiEpisodeStyle { get; set; }

        public SelectList SeparatorStyles { get; set; }
        public SelectList NumberStyles { get; set; }
        public SelectList MultiEpisodeStyles { get; set; }
    }
}