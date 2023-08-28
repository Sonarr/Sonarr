using System.Collections.Generic;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public SubtitleFile()
        {
            LanguageTags = new List<string>();
        }

        public Language Language { get; set; }

        public string AggregateString => Language + Title + LanguageTagsAsString + Extension;

        public int Copy { get; set; }

        public List<string> LanguageTags { get; set; }

        public string Title { get; set; }

        private string LanguageTagsAsString => string.Join(".", LanguageTags);

        public override string ToString()
        {
            return $"[{Id}] {RelativePath} ({Language}{(Title is not null ? "." : "")}{Title ?? ""}{(LanguageTags.Count > 0 ? "." : "")}{LanguageTagsAsString}{Extension})";
        }
    }
}
