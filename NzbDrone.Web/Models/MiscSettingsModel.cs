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

        [DisplayName("Automatically Ignore Deleted Episodes")]
        [Description("Deleted episodes are automatically ignored.")]
        public bool AutoIgnorePreviouslyDownloadedEpisodes { get; set; }

        [DisplayName("Specified Release Groups")]
        [Description("Comma separated list of release groups to download episodes (leave empty for all groups)")]
        public string AllowedReleaseGroups { get; set; }
    }
}