using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NzbDrone.Core.Model;

namespace NzbDrone.Web.Models
{
    public class MiscSettingsModel
    {
        [DisplayName("Enable Backlog Searching")]
        [Description("Should NzbDrone try to download missing episodes automatically?")]
        public bool EnableBacklogSearching { get; set; }
    }
}