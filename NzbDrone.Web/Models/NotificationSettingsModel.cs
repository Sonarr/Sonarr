using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class NotificationSettingsModel
    {
        [DisplayName("Enabled")]
        public bool XbmcEnabled { get; set; }

        [DisplayName("Notify on Grab")]
        public bool XbmcNotifyOnGrab { get; set; }

        [DisplayName("Notify on Download")]
        public bool XbmcNotifyOnDownload { get; set; }

        [DisplayName("Notify on Rename")]
        public bool XbmcNotifyOnRename { get; set; }

        [DisplayName("Image with Notification")]
        public bool XbmcNotificationImage { get; set; }

        [Required]
        [Range(3, 10, ErrorMessage = "Must be between 3 and 10 seconds")]
        [DisplayName("Display Time")]
        public int XbmcDisplayTime { get; set; }

        [DisplayName("Update on Download")]
        public bool XbmcUpdateOnDownload { get; set; }

        [DisplayName("Update on Rename")]
        public bool XbmcUpdateOnRename { get; set; }

        [DisplayName("Update on ")]
        public bool XbmcFullUpdate { get; set; }

        [DisplayName("Clean on Download")]
        public bool XbmcCleanOnDownload { get; set; }

        [DisplayName("Clean on Rename")]
        public bool XbmcCleanOnRename { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Hosts")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string XbmcHosts { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string XbmcUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Password")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string XbmcPassword { get; set; }
    }
}