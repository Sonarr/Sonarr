using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Instrumentation.Commands
{
    public class DeleteUpdateLogFilesCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
