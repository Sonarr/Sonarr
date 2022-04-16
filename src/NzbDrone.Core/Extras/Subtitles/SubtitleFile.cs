using System.Collections.Generic;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public Language Language { get; set; }

        public string AggregateString => Language + Extension + LanguageTags;

        public string LanguageTags { get; set; }

        public void SetLanguageTags(IEnumerable<string> newTags)
        {
            LanguageTags = string.Join(".", newTags);
        }

        public string FullPath { get; set; }
    }
}
