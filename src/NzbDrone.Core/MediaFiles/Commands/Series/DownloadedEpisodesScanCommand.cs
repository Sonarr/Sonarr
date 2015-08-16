using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands.Series
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

        // Properties used by third-party apps, do not modify.
        public String Path { get; set; }
        public String DownloadClientId { get; set; }
    }
}