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
        [DisplayName("Username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbMatrixUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("API Key")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbMatrixApiKey { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("UID")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsOrgUId { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Hash")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsOrgHash { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("UID")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsrusUId { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Hash")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsrusHash { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NewzbinUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Password")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NewzbinPassword { get; set; }

        [DisplayName("NZBs.org")]
        public bool NzbsOrgEnabled { get; set; }

        [DisplayName("NZB Matrix")]
        public bool NzbMatrixEnabled { get; set; }

        [DisplayName("NZBsRUs")]
        public bool NzbsRUsEnabled { get; set; }

        [DisplayName("Newzbin")]
        public bool NewzbinEnabled { get; set; }
    }
}