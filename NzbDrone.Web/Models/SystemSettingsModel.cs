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
    public class SystemSettingsModel
    {
        [DisplayName("Port")]
        [Description("Port that NzbDrone runs on")]
        [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
        public int Port { get; set; }

        [DisplayName("Launch Browser")]
        [Description("Start default webrowser when NzbDrone starts?")]
        public bool LaunchBrowser { get; set; }

        [DisplayName("Authentication")]
        [Description("Secure the webserver with Authentication?")]
        public AuthenticationType AuthenticationType { get; set; }

        public SelectList AuthTypeSelectList { get; set; }
    }
}