using System.Collections.Generic;
using NzbDrone.Core.Extras.Files;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class ImportedSubtitleFiles
    {
        public List<string> SourceFiles { get; set; }
        public List<ExtraFile> SubtitleFiles { get; set; }

        public ImportedSubtitleFiles()
        {
            SourceFiles = new List<string>();
            SubtitleFiles = new List<ExtraFile>();
        }
    }
}
