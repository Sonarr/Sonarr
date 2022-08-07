using System.Collections.Generic;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public Language Language { get; set; }

        public string AggregateString => Language + LanguageTagsAsString + Extension;

        public List<string> LanguageTags { get; set; }

        private string LanguageTagsAsString => string.Join(".", LanguageTags);
    }
}
