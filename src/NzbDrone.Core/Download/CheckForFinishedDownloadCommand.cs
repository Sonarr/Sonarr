using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Download
{
    public class CheckForFinishedDownloadCommand : Command
    {
        public override bool RequiresDiskAccess => true;
    }
}
