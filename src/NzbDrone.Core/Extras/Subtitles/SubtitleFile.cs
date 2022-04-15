using System.Collections.ObjectModel;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public Language Language { get; set; }

        public Collection<string> Tags { get; set; }

        public string AggregateString => Language + Extension + TagsString;

        public string TagsString => string.Join(".", Tags);
    }
}
