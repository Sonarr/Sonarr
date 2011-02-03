using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Web.Models
{

    public class SettingsModel
    {

        #region General Settings
        [DataType(DataType.Text)]
        [DisplayName("TV Folder")]
        public String TvFolder
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("Initial Quality")]
        public int Quality
        {
            get;
            set;
        }

        #endregion

        #region Indexer Settings

        [DataType(DataType.Text)]
        [DisplayName("NZBMatrix Username")]
        public String NzbMatrixUsername
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("NZBMatrix API Key")]
        public String NzbMatrixApiKey
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("NZBs.Org UID")]
        public String NzbsOrgUId
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("NZBs.Org Hash")]
        public String NzbsOrgHash
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("NZBsRus UID")]
        public String NzbsrusUId
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("NZBsRus Hash")]
        public String NzbsrusHash
        {
            get;
            set;
        }

        public List<Indexer> Indexers
        {
            get;
            set;
        }
        #endregion

        #region Download Settings

        //Sync Frequency
        //Download Propers?
        //Retention
        //SAB Host/IP
        //SAB Port
        //SAB APIKey
        //SAB Username
        //SAB Password
        //SAB Category
        //SAB Priority

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

        [DataType(DataType.Text)]
        [DisplayName("Retention")]
        public int Retention
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Host")]
        public String SabHost
        {
            get;
            set;
        }

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

        [DataType(DataType.Text)]
        [DisplayName("SABnzbd Priority")]
        public SabnzbdPriorityType SabPriority
        {
            get;
            set;
        }

        #endregion
    }

}
