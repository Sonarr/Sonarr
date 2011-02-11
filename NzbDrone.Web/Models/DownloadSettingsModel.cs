using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Model;

namespace NzbDrone.Web.Models
{
    public class DownloadSettingsModel
    {
        [Required]
        [Range(15, 120, ErrorMessage = "Must be between 15 and 120 minutes")]
        [DataType(DataType.Text)]
        [DisplayName("Sync Frequency")]
        public int SyncFrequency
        {
            get;
            set;
        }

        [DisplayName("Download Propers")]
        public bool DownloadPropers
        {
            get;
            set;
        }

        [Required (ErrorMessage = "Please enter a valid number")]
        [DataType(DataType.Text)]
        [DisplayName("Retention")]
        public int Retention
        {
            get;
            set;
        }

        [Required (ErrorMessage = "Please enter a valid host")]
        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Host")]
        public String SabHost
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Please enter a valid port")]
        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Port")]
        public int SabPort
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd API Key")]
        public String SabApiKey
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Username")]
        public String SabUsername
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Password")]
        public String SabPassword
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Category")]
        public String SabCategory
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Please select a valid priority")]
        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Priority")]
        public SabnzbdPriorityType SabPriority
        {
            get;
            set;
        }

        public SelectList PrioritySelectList = new SelectList(new string[] { "Low", "Normal", "High" });
    }
}