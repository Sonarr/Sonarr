using Workarr.Messaging;

namespace Workarr.Update.Events
{
    public class UpdateInstalledEvent : IEvent
    {
        public Version PreviousVerison { get; set; }
        public Version NewVersion { get; set; }

        public UpdateInstalledEvent(Version previousVersion, Version newVersion)
        {
            PreviousVerison = previousVersion;
            NewVersion = newVersion;
        }
    }
}
