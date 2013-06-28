using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexClientSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "Host")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Username")]
        public String Username { get; set; }

        [FieldDefinition(3, Label = "Password")]
        public String Password { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Host);
            }
        }
    }
}
