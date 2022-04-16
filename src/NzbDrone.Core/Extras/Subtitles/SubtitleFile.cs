using System.Collections.Generic;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public Language Language { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public string AggregateString => Language + Extension + TagsString;

        public string TagsString => string.Join(".", Tags);
        
        public string FullPath { get; set; }
    }
}
