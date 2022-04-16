using System.Collections.Generic;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public Language Language { get; set; }

        public string AggregateString => Language + Extension + Tags;

        public string Tags { get; set; }

        public void SetTags(IEnumerable<string> newTags)
        {
            Tags = string.Join(".", newTags);
        }

        public IEnumerable<string> TagsEnumerable => Tags.Split('.');
        
        public string FullPath { get; set; }
    }
}
