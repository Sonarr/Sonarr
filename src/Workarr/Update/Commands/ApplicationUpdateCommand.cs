using Workarr.Messaging.Commands;

namespace Workarr.Update.Commands
{
    public class ApplicationUpdateCommand : Command
    {
        public bool InstallMajorUpdate { get; set; }
        public override bool SendUpdatesToClient => true;
        public override bool IsExclusive => true;
    }
}
