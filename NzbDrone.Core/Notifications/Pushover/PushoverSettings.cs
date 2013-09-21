using System;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.Pushover
{
    public class PushoverSettings : IProviderConfig
    {
        [FieldDefinition(0, Label = "User Key", HelpLink = "https://pushover.net/")]
        public String UserKey { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(PushoverPriority) )]
        public Int32 Priority { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(UserKey) && Priority != null & Priority >= -1 && Priority <= 2;
            }
        }
    }
}
