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
        [Description("Username for NZB Matrix")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbMatrixUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("API Key")]
        [Description("API Key for NZB Matrix")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbMatrixApiKey { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("UID")]
        [Description("User ID for Nzbs.org")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsOrgUId { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Hash")]
        [Description("Hash for Nzbs.org")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsOrgHash { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("UID")]
        [Description("User ID for NZBsRus")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsrusUId { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Hash")]
        [Description("Hash for NZBsRus")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NzbsrusHash { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Username")]
        [Description("Username for Newzbin")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NewzbinUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Password")]
        [Description("Password for Newzbin")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String NewzbinPassword { get; set; }

        [DisplayName("NZBs.org")]
        [Description("Scan Nzbs.org for new epsiodes")]
        public bool NzbsOrgEnabled { get; set; }

        [DisplayName("NZB Matrix")]
        [Description("Scan NZB Matrix for new epsiodes")]
        public bool NzbMatrixEnabled { get; set; }

        [DisplayName("NZBsRUs")]
        [Description("Scan NZBsRus for new epsiodes")]
        public bool NzbsRUsEnabled { get; set; }

        [DisplayName("Newzbin")]
        [Description("Scan Newzbin for new epsiodes")]
        public bool NewzbinEnabled { get; set; }
    }
}