using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NzbDrone.Core.Repository;

namespace NzbDrone.Web.Models
{
    public class IndexerSettingsModel
    {
        [DataType(DataType.Text)]
        [DisplayName("NZBMatrix Username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbMatrixUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("NZBMatrix API Key")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbMatrixApiKey { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("NZBs.Org UID")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsOrgUId { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("NZBs.Org Hash")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsOrgHash { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("NZBsRus UID")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsrusUId { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("NZBsRus Hash")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsrusHash { get; set; }

        public List<Indexer> Indexers { get; set; }
    }
}