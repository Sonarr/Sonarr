using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Common.Model;

namespace NzbDrone.Web.Models
{
    public class SystemSettingsModel
    {
        [DisplayName("Port")]
        [Description("Port that NzbDrone runs on")]
        [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
        [Required(ErrorMessage = "Port must be between 1 and 65535")]
        public int Port { get; set; }

        [DisplayName("Launch Browser")]
        [Description("Start web browser when NzbDrone starts?")]
        public bool LaunchBrowser { get; set; }

        [DisplayName("Authentication")]
        [Description("Secure the server with authentication?")]
        public AuthenticationType AuthenticationType { get; set; }

        public SelectList AuthTypeSelectList { get; set; }

        [DisplayName("Recycle Bin")]
        [Description("Path to NzbDrone's internal recycle bin (optional)")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RecycleBin { get; set; }
    }
}