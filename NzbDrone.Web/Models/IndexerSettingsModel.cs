using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NzbDrone.Core.Repository;
using NzbDrone.Web.Helpers.Validation;

namespace NzbDrone.Web.Models
{
    public class IndexerSettingsModel
    {
        [DataType(DataType.Text)]
        [DisplayName("Username")]
        [Description("Username for NZB Matrix")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NzbMatrixEnabled", true, ErrorMessage = "USername Required when NZBMatrix is enabled")]
        public String NzbMatrixUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("API Key")]
        [Description("API Key for NZB Matrix")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NzbMatrixEnabled", true, ErrorMessage = "API Key Required when NZBMatrix is enabled")]
        public String NzbMatrixApiKey { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("UID")]
        [Description("User ID for Nzbs.org")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NzbsOrgEnabled", true, ErrorMessage = "UID Required when Nzbs.org is enabled")]
        public String NzbsOrgUId { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Hash")]
        [Description("Hash for Nzbs.org")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NzbsOrgEnabled", true, ErrorMessage = "Hash Required when Nzbs.org is enabled")]
        public String NzbsOrgHash { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("UID")]
        [Description("User ID for NZBsRus")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NzbsRUsEnabled", true, ErrorMessage = "UID Required when NzbsRus is enabled")]
        public String NzbsrusUId { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Hash")]
        [Description("Hash for NZBsRus")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NzbsRUsEnabled", true, ErrorMessage = "Hash Required when NzbsRus is enabled")]
        public String NzbsrusHash { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Username")]
        [Description("Username for Newzbin")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NewzbinEnabled", true, ErrorMessage = "Username Required when Newzbin is enabled")]
        public String NewzbinUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Password")]
        [Description("Password for Newzbin")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NewzbinEnabled", true, ErrorMessage = "Password Required when Newzbin is enabled")]
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

        [Required(ErrorMessage = "Please enter a valid number of days")]
        [DataType(DataType.Text)]
        [DisplayName("Retention")]
        [Description("Usenet provider retention in days (0 = unlimited)")]
        public int Retention { get; set; }

        public List<NewznabDefinition> NewznabDefinitions { get; set; }
    }
}