using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Growl
{
    public class GrowlSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "Host", HelpText = "Growl Host (IP or Hostname)")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port", HelpText = "Growl Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Password", HelpText = "Password for Growl")]
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
