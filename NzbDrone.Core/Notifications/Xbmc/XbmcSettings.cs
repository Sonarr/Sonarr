using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class XbmcSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "Host", HelpText = "XBMC Hostnname or IP")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port", HelpText = "Webserver port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Username", HelpText = "Webserver Username")]
        public String Username { get; set; }

        [FieldDefinition(3, Label = "Password", HelpText = "Webserver Password ")]
        public String Password { get; set; }

        [FieldDefinition(4, Label = "Update Library", HelpText = "Update Library on Download & Rename?")]
        public Boolean UpdateLibrary { get; set; }

        [FieldDefinition(5, Label = "Update Library", HelpText = "Clean Library after update?")]
        public Boolean CleanLibrary { get; set; }

        [FieldDefinition(6, Label = "Always Update", HelpText = "Update Library even when a video is playing?")]
        public Boolean AlwaysUpdate { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Host) && Port > 0;
            }
        }
    }
}
