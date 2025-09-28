using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Profiles.Qualities.Commands
{
    public class ApplyQualityProfileDowngradeCommand : Command
    {
        public override bool UpdateScheduledTask => true;
        public override string CompletionMessage => "Completed";
    }
}

