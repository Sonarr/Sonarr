using NzbDrone.Core.Annotations;
using NzbDrone.Core.Notifications.Xbmc;

namespace NzbDrone.Core.Notifications.Plex.HomeTheater
{
    public class PlexHomeTheaterSettings : XbmcSettings
    {
        public PlexHomeTheaterSettings()
        {
            Port = 3005;
        }

        //These need to be kept in the same order as XBMC Settings, but we don't want them displayed

        [FieldDefinition(4, Label = "Username", Hidden = HiddenType.Hidden)]
        public new string Username { get; set; }

        [FieldDefinition(5, Label = "Password", Hidden = HiddenType.Hidden)]
        public new string Password { get; set; }

        [FieldDefinition(7, Label = "GUI Notification", Type = FieldType.Checkbox, Hidden = HiddenType.Hidden)]
        public new bool Notify { get; set; } = true;

        [FieldDefinition(8, Label = "Update Library", HelpText = "Update Library on Import & Rename?", Type = FieldType.Checkbox, Hidden = HiddenType.Hidden)]
        public new bool UpdateLibrary { get; set; }

        [FieldDefinition(9, Label = "Clean Library", HelpText = "Clean Library after update?", Type = FieldType.Checkbox, Hidden = HiddenType.Hidden)]
        public new bool CleanLibrary { get; set; }

        [FieldDefinition(10, Label = "Always Update", HelpText = "Update Library even when a video is playing?", Type = FieldType.Checkbox, Hidden = HiddenType.Hidden)]
        public new bool AlwaysUpdate { get; set; }
    }
}
