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
        public int SyncFrequency { get; set; }

        [DisplayName("Download Propers")]
        public bool DownloadPropers { get; set; }

        [Required(ErrorMessage = "Please enter a valid number")]
        [DataType(DataType.Text)]
        [DisplayName("Retention")]
        public int Retention { get; set; }

        [Required(ErrorMessage = "Please enter a valid host")]
        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Host")]
        public String SabHost { get; set; }

        [Required(ErrorMessage = "Please enter a valid port")]
        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Port")]
        public int SabPort { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd API Key")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String SabApiKey { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String SabUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Password")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String SabPassword { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DisplayName("SABnzbd TV Category")]
        public String SabTvCategory { get; set; }

        [Required(ErrorMessage = "Please select a valid priority")]
        [DisplayName("SABnzbd Priority")]
        public SabnzbdPriorityType SabTvPriority { get; set; }

        [DisplayName("Use Blackhole")]
        public bool UseBlackHole { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DisplayName("Blackhole Directory")]
        public String BlackholeDirectory { get; set; }
    }
}