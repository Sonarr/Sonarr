using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NzbDrone.Web.Models
{
    public class NotificationSettingsModel
    {
        [DisplayName("Enabled")]
        [Description("Enable notifications for XBMC?")]
        public bool XbmcEnabled { get; set; }

        [DisplayName("Notify on Grab")]
        [Description("Send notification when episode is sent to SABnzbd?")]
        public bool XbmcNotifyOnGrab { get; set; }

        [DisplayName("Notify on Download")]
        [Description("Send notification when episode is downloaded?")]
        public bool XbmcNotifyOnDownload { get; set; }

        [DisplayName("Notify on Rename")]
        [Description("Send notification when episode is renamed?")]
        public bool XbmcNotifyOnRename { get; set; }

        [DisplayName("Image with Notification")]
        [Description("Display NzbDrone image on notifications?")]
        public bool XbmcNotificationImage { get; set; }

        [Required]
        [Range(3, 10, ErrorMessage = "Must be between 3 and 10 seconds")]
        [DisplayName("Display Time")]
        [Description("How long the notification should be displayed")]
        public int XbmcDisplayTime { get; set; }

        [DisplayName("Update on Download")]
        [Description("Update XBMC library after episode download?")]
        public bool XbmcUpdateOnDownload { get; set; }

        [DisplayName("Update on Rename")]
        [Description("Update XBMC library after episode is renamed?")]
        public bool XbmcUpdateOnRename { get; set; }

        [DisplayName("Full Update")]
        [Description("Perform a full update is series update fails?")]
        public bool XbmcFullUpdate { get; set; }

        [DisplayName("Clean on Download")]
        [Description("Clean XBMC library after episode download?")]
        public bool XbmcCleanOnDownload { get; set; }

        [DisplayName("Clean on Rename")]
        [Description("Clean XBMC library after episode is renamed?")]
        public bool XbmcCleanOnRename { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Hosts")]
        [Description("XBMC hosts with port, comma separ")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string XbmcHosts { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Username")]
        [Description("XBMC webserver username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string XbmcUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Password")]
        [Description("XBMC webserver password")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string XbmcPassword { get; set; }
    }
}