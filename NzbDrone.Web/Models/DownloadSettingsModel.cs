using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NzbDrone.Core.Model;

namespace NzbDrone.Web.Models
{
    public class DownloadSettingsModel
    {
        public SelectList PrioritySelectList =
            new SelectList(new[] {"Default", "Paused", "Low", "Normal", "High", "Top"});

        [Required]
        [Range(15, 120, ErrorMessage = "Must be between 15 and 120 minutes")]
        [DataType(DataType.Text)]
        [DisplayName("Sync Frequency")]
        [Description("Specifies how often NzbDrone should check RSS Feeds (Minutes)")]
        public int SyncFrequency { get; set; }

        [DisplayName("Download Propers")]
        [Description("Should NzbDrone download proper releases (to replace non-proper files)?")]
        public bool DownloadPropers { get; set; }

        [Required(ErrorMessage = "Please enter a valid number")]
        [DataType(DataType.Text)]
        [DisplayName("Retention")]
        [Description("Your newsgroup provider retention (Days)")]
        public int Retention { get; set; }

        [Required(ErrorMessage = "Please enter a valid host")]
        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Host")]
        [Description("Hostname or IP Address running SABnzbd")]
        public String SabHost { get; set; }

        [Required(ErrorMessage = "Please enter a valid port")]
        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Port")]
        [Description("Port for SABnzbd web interface")]
        public int SabPort { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd API Key")]
        [Description("API Key for SABNzbd, Found in Config: General")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String SabApiKey { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Username")]
        [Description("Username for SABnzbd WebUI (Not Required when using APIKey)")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String SabUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Password")]
        [Description("Password for SABnzbd WebUI (Not required when using APIKey)")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String SabPassword { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DisplayName("SABnzbd TV Category")]
        [Description("Category to use when sending NZBs to SABnzbd")]
        public String SabTvCategory { get; set; }

        [Required(ErrorMessage = "Please select a valid priority")]
        [DisplayName("SABnzbd Priority")]
        [Description("Priority to use when sending NZBs to SABnzbd")]
        public SabnzbdPriorityType SabTvPriority { get; set; }

        [DisplayName("Use Blackhole")]
        [Description("Whether to use the blackhole directory instead of sending NZB to SABnzbd")]
        public bool UseBlackHole { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DisplayName("Blackhole Directory")]
        [Description("Path to the blackhole directory")]
        public String BlackholeDirectory { get; set; }
    }
}