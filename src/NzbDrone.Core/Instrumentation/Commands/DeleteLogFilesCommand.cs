using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Instrumentation.Commands
{
    public class DeleteLogFilesCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
