using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class XbmcSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "Host")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Username")]
        public String Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password)]
        public String Password { get; set; }

        [FieldDefinition(4, Label = "GUI Notification", HelpText = "Show GUI notifications?", Type = FieldType.Checkbox)]
        public Boolean Notify { get; set; }

        [FieldDefinition(5, Label = "Update Library", HelpText = "Update Library on Download & Rename?", Type = FieldType.Checkbox)]
        public Boolean UpdateLibrary { get; set; }

        [FieldDefinition(6, Label = "Clean Library", HelpText = "Clean Library after update?", Type = FieldType.Checkbox)]
        public Boolean CleanLibrary { get; set; }

        [FieldDefinition(7, Label = "Always Update", HelpText = "Update Library even when a video is playing?", Type = FieldType.Checkbox)]
        public Boolean AlwaysUpdate { get; set; }

        [JsonIgnore]
        public String Address { get { return String.Format("{0}:{1}", Host, Port); } }
        
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Host) && Port > 0;
            }
        }
    }
}
