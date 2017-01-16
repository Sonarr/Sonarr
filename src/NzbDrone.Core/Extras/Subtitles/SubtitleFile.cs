using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public Language Language { get; set; }
    }
}
