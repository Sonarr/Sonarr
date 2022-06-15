using System.Collections.Generic;
using System.IO;
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

        public void SetRelativePath(string fullPath, string seriesPath)
        {
            var endsWithSeparator = seriesPath.EndsWith(Path.PathSeparator.ToString());
            var startPos = endsWithSeparator ? seriesPath.Length : seriesPath.Length + 1;
            RelativePath = fullPath.Substring(startPos);
        }
    }
}
