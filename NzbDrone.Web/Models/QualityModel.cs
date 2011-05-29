using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Web.Models
{
    public class QualityModel
    {
        public List<QualityProfile> Profiles { get; set; }

        [DisplayName("Default Quality Profile")]
        [Description("The default quality to use when adding series to NzbDrone")]
        public int DefaultQualityProfileId { get; set; }

        [DisplayName("Download Propers")]
        [Description("Should NzbDrone download proper releases (to replace non-proper files)?")]
        public bool DownloadPropers { get; set; }

        public SelectList SelectList { get; set; }
    }
}