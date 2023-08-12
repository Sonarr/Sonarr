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

        public string AggregateString => Language + LanguageTagsAsString + Extension;

        public List<string> LanguageTags { get; set; }

        private string LanguageTagsAsString => string.Join(".", LanguageTags);

        public override string ToString()
        {
            return $"[{Id}] {RelativePath} ({Language}{(LanguageTags.Count > 0 ? "." : "")}{LanguageTagsAsString}{Extension})";
        }
    }
}
