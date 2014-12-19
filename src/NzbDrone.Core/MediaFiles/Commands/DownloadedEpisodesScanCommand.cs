using System;
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

        public Boolean SendUpdates { get; set; }
    }
}