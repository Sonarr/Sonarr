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
        [Description("Enable downloading episodes from Nzbs.org")]
        public bool NzbsOrgEnabled { get; set; }

        [DisplayName("NZB Matrix")]
        [Description("Enable downloading episodes from NZB Matrix")]
        public bool NzbMatrixEnabled { get; set; }

        [DisplayName("NZBsRUs")]
        [Description("Enable downloading episodes from NZBsRus")]
        public bool NzbsRUsEnabled { get; set; }

        [DisplayName("Newzbin")]
        [Description("Enable downloading episodes from Newzbin")]
        public bool NewzbinEnabled { get; set; }

        [DisplayName("Newznab")]
        [Description("Enable downloading episodes from Newznab Providers")]
        public bool NewznabEnabled { get; set; }

        public List<NewznabDefinition> NewznabDefinitions { get; set; }
    }
}