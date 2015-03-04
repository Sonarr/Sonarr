using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Manual
{
    public class ManualImportCommand : Command
    {
        public List<ManualImportFile> Files { get; set; }
    }
}
