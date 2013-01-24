using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Nzbget;
using NzbDrone.Core.Model.Sabnzbd;
using NzbDrone.Web.Helpers.Validation;

namespace NzbDrone.Web.Models
{
    public class DownloadClientSettingsModel
    {
        public SelectList SabPrioritySelectList =
            new SelectList(new[] {"Default", "Paused", "Low", "Normal", "High", "Force"});

        public SelectList NzbgetPrioritySelectList { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Host")]
        [Description("Hostname or IP Address running SABnzbd")]
        [RequiredIf("DownloadClient", (int)DownloadClientType.Sabnzbd, ErrorMessage = "Required when Download Client is SABnzbd")]
        public String SabHost { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Port")]
        [Description("Port for SABnzbd web interface")]
        [RequiredIf("DownloadClient", (int)DownloadClientType.Sabnzbd, ErrorMessage = "Required when Download Client is SABnzbd")]
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
        [DisplayName("SABnzbd Backlog Priority")]
        [Description("Priority to use when sending episodes older than 7 days to SABnzbd")]
        public SabPriorityType SabBacklogTvPriority { get; set; }

        [Required(ErrorMessage = "Please select a valid priority")]
        [DisplayName("SABnzbd Recent Priority")]
        [Description("Priority to use when sending episodes newer than 7 days to SABnzbd")]
        public SabPriorityType SabRecentTvPriority { get; set; }

        [Required(ErrorMessage = "Required so NzbDrone can sort downloads")]
        [DisplayName("Download Client TV Directory")]
        [Description("The directory where your download client downloads TV shows to")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DownloadClientDropDirectory { get; set; }

        [DisplayName("Blackhole Directory")]
        [Description("The directory where your download client will pickup NZB files")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("DownloadClient", (int)DownloadClientType.Blackhole, ErrorMessage = "Required when Download Client is Blackhole")]
        public string BlackholeDirectory { get; set; }

        [DisplayName("Download Client")]
        [Description("What method do you download NZBs with?")]
        public int DownloadClient { get; set; }

        [DisplayName("Pneumatic Nzb Directory")]
        [Description("Directory to save NZBs for Pneumatic, must be able from XBMC")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("DownloadClient", (int)DownloadClientType.Pneumatic, ErrorMessage = "Required when Download Client is Blackhole")]
        public string PneumaticDirectory { get; set; }

        [DisplayName("Use Scene Name")]
        [Description("Use Scene name when adding NZB to queue?")]
        public Boolean UseSceneName { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Nzbget Host")]
        [Description("Hostname or IP Address running Nzbget")]
        [RequiredIf("DownloadClient", (int)DownloadClientType.Nzbget, ErrorMessage = "Required when Download Client is Nzbget")]
        public String NzbgetHost { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Nzbget Port")]
        [Description("Port for Nzbget web interface")]
        [RequiredIf("DownloadClient", (int)DownloadClientType.Nzbget, ErrorMessage = "Required when Download Client is Nzbget")]
        public int NzbgetPort { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Nzbget Username")]
        [Description("Username for Nzbget")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbgetUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Nzbget Password")]
        [Description("Password for Nzbget")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbgetPassword { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DisplayName("Nzbget TV Category")]
        [Description("Category to use when sending NZBs to Nzbget")]
        public String NzbgetTvCategory { get; set; }

        [Required(ErrorMessage = "Please select a valid priority")]
        [DisplayName("Nzbget Backlog Priority")]
        [Description("Priority to use when sending episodes older than 7 days to Nzbget")]
        public Int32 NzbgetBacklogTvPriority { get; set; }

        [Required(ErrorMessage = "Please select a valid priority")]
        [DisplayName("Nzbget Recent Priority")]
        [Description("Priority to use when sending episodes newer than 7 days to Nzbget")]
        public Int32 NzbgetRecentTvPriority { get; set; }

        public SelectList SabTvCategorySelectList { get; set; }
        public SelectList DownloadClientSelectList { get; set; }
    }
}