using Workarr.Messaging.Commands;

namespace Workarr.Instrumentation.Commands
{
    public class ClearLogCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
