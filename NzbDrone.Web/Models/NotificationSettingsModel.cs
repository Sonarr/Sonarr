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

        [DisplayName("Update on Download and Rename")]
        [Description("Update XBMC library after episode is downloaded or renamed?")]
        public bool XbmcUpdateLibrary { get; set; }

        [DisplayName("Clean on Download/Rename")]
        [Description("Clean XBMC library after an episode is downloaded or renamed?")]
        public bool XbmcCleanLibrary { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Hosts")]
        [Description("XBMC hosts with port, comma separated")]
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