using System;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Growl
{
    public class GrowlSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "Host")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Password")]
        public String Password { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Password) && Port > 0;
            }
        }
    }
}
