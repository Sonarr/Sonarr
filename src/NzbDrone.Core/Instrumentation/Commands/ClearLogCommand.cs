using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Instrumentation.Commands
{
    public class ClearLogCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
