using Workarr.Messaging.Commands;

namespace Workarr.Update.Commands
{
    public class ApplicationUpdateCheckCommand : Command
    {
        public override bool SendUpdatesToClient => true;

        public override string CompletionMessage => null;

        public bool InstallMajorUpdate { get; set; }
    }
}
