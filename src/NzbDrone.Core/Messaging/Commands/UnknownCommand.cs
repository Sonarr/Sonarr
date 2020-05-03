namespace NzbDrone.Core.Messaging.Commands
{
    public class UnknownCommand : Command
    {
        public override bool SendUpdatesToClient => false;

        public override string CompletionMessage => "Skipped";

        public string ContractName { get; set; }
    }
}
