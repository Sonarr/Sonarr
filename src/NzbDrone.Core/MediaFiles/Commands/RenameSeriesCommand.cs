using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameSeriesCommand : Command
    {
        public List<int> SeriesIds { get; set; }

        public override bool SendUpdatesToClient => true;

        public RenameSeriesCommand()
        {
            SeriesIds = new List<int>();
        }
    }
}
