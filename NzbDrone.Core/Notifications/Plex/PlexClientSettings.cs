using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexClientSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "Host", HelpText = "Plex Client Host (IP or Hostname)")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port", HelpText = "Plex Client Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Username", HelpText = "Username for Plex")]
        public String Username { get; set; }

        [FieldDefinition(3, Label = "Password", HelpText = "Password for Plex")]
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
