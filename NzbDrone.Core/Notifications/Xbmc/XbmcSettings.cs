using System;
using System.ComponentModel;
using Newtonsoft.Json;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class XbmcSettings : IProviderConfig
    {
        public XbmcSettings()
        {
            DisplayTime = 5;
            Port = 8080;
        }

        [FieldDefinition(0, Label = "Host")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Username")]
        public String Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password)]
        public String Password { get; set; }

        [DefaultValue(5)]
        [FieldDefinition(4, Label = "Display Time", HelpText = "How long the notification will be displayed for (In seconds)")]
        public Int32 DisplayTime { get; set; }

        [FieldDefinition(5, Label = "GUI Notification", Type = FieldType.Checkbox)]
        public Boolean Notify { get; set; }

        [FieldDefinition(6, Label = "Update Library", HelpText = "Update Library on Download & Rename?", Type = FieldType.Checkbox)]
        public Boolean UpdateLibrary { get; set; }

        [FieldDefinition(7, Label = "Clean Library", HelpText = "Clean Library after update?", Type = FieldType.Checkbox)]
        public Boolean CleanLibrary { get; set; }

        [FieldDefinition(8, Label = "Always Update", HelpText = "Update Library even when a video is playing?", Type = FieldType.Checkbox)]
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
