using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using NzbDrone.Core.Repository;

namespace NzbDrone.Web.Models
{
    public class IndexerSettingsModel
    {
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
    }
}