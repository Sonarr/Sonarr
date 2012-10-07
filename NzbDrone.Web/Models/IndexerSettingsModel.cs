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
        [RequiredIf("NzbMatrixEnabled", true, ErrorMessage = "Username Required when NZBMatrix is enabled")]
        public String NzbMatrixUsername { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("API Key")]
        [Description("API Key for NZB Matrix")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NzbMatrixEnabled", true, ErrorMessage = "API Key Required when NZBMatrix is enabled")]
        public String NzbMatrixApiKey { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("User ID")]
        [Description("User ID for NZBsRus")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NzbsRUsEnabled", true, ErrorMessage = "User ID Required when NzbsRus is enabled")]
        public String NzbsrusUId { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("API Key")]
        [Description("API Key for NZBsRus")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("NzbsRUsEnabled", true, ErrorMessage = "API Key Required when NzbsRus is enabled")]
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

        [DataType(DataType.Text)]
        [DisplayName("UID")]
        [Description("UserID for File Sharing Talk")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("FileSharingTalkEnabled", true, ErrorMessage = "UserID Required when File Sharing Talk is enabled")]
        public String FileSharingTalkUid { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Secret")]
        [Description("Password Secret for File Sharing Talk")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RequiredIf("FileSharingTalkEnabled", true, ErrorMessage = "Password Secret Required when File Sharing Talk is enabled")]
        public String FileSharingTalkSecret { get; set; }

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

        [DisplayName("Womble's Index")]
        [Description("Enable downloading episodes from Womble's Index")]
        public bool WomblesEnabled { get; set; }

        [DisplayName("File Sharing Talk")]
        [Description("Enable downloading episodes from File Sharing Talk")]
        public bool FileSharingTalkEnabled { get; set; }

        [DisplayName("NzbIndex")]
        [Description("Enable downloading episodes from NzbIndex")]
        public bool NzbIndexEnabled { get; set; }

        [DisplayName("NzbClub")]
        [Description("Enable downloading episodes from NzbClub")]
        public bool NzbClubEnabled { get; set; }

        [Required(ErrorMessage = "Please enter a valid number of days")]
        [DataType(DataType.Text)]
        [DisplayName("Retention")]
        [Description("Usenet provider retention in days (0 = unlimited)")]
        public int Retention { get; set; }

        [DisplayName("RSS Sync Interval")]
        [Description("Check for new episodes every X minutes")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter a valid time in minutes")]
        [Range(15, 240, ErrorMessage = "Interval must be between 15 and 240 minutes")]
        public int RssSyncInterval { get; set; }

        public List<NewznabDefinition> NewznabDefinitions { get; set; }
    }
}