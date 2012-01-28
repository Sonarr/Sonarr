using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Sabnzbd;

namespace NzbDrone.Web.Models
{
    public class DownloadClientSettingsModel
    {
        public SelectList PrioritySelectList =
            new SelectList(new[] {"Default", "Paused", "Low", "Normal", "High", "Top"});

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
        public SabPriorityType SabTvPriority { get; set; }

        [DisplayName("Download Client TV Directory")]
        [Description("The directory where your download client downloads TV shows to (NzbDrone will sort them for you)")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DownloadClientDropDirectory { get; set; }

        [DisplayName("Blackhole Directory")]
        [Description("The directory where your download client will pickup NZB files")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string BlackholeDirectory { get; set; }

        [DisplayName("Download Client")]
        [Description("What method do you download NZBs with?")]
        public int DownloadClient { get; set; }

        public SelectList SabTvCategorySelectList { get; set; }
        public SelectList DownloadClientSelectList { get; set; }
    }
}