using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class DownloadedEpisodesScanCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return SendUpdates;
            }
        }

        public bool SendUpdates { get; set; }
    }
}