using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NzbDrone.Web.Models
{
    public class NotificationSettingsModel
    {
        //XBMC
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

        //SMTP
        [DisplayName("Enabled")]
        [Description("Enable SMTP notifications?")]
        public bool SmtpEnabled { get; set; }

        [DisplayName("Notify on Grab")]
        [Description("Send notification when episode is sent to SABnzbd?")]
        public bool SmtpNotifyOnGrab { get; set; }

        [DisplayName("Notify on Download")]
        [Description("Send notification when episode is downloaded?")]
        public bool SmtpNotifyOnDownload { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Server")]
        [Description("SMTP Server Hostname")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SmtpServer{ get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Port")]
        [Description("SMTP Server Port")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public int SmtpPort { get; set; }

        [DisplayName("SSL")]
        [Description("Does the SMTP Server use SSL?")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public bool SmtpUseSsl { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Username")]
        [Description("SMTP Server authentication username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SmtpUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Password")]
        [Description("SMTP Server authentication password")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SmtpPassword { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Send From Address")]
        [Description("Sender Email address")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SmtpFromAddress { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Send To Addresses")]
        [Description("Comma separated list of addresses to email")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SmtpToAddresses { get; set; }
    }
}