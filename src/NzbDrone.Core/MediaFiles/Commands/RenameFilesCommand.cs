using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameFilesCommand : Command
    {
        public int SeriesId { get; set; }
        public List<int> Files { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public RenameFilesCommand()
        {
        }

        public RenameFilesCommand(int seriesId, List<int> files)
        {
            SeriesId = seriesId;
            Files = files;
        }
    }
}