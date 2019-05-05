using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Download
{
    public class ProcessMonitoredDownloadsCommand : Command
    {
        public override bool RequiresDiskAccess => true;
    }
}
