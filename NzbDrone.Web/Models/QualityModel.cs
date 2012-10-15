using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Web.Models
{
    public class QualityModel
    {
        public List<QualityProfileModel> Profiles { get; set; }

        [DisplayName("Default Quality Profile")]
        [Description("The default quality to use when adding series to NzbDrone")]
        public int DefaultQualityProfileId { get; set; }

        public SelectList QualityProfileSelectList { get; set; }

        public int SdtvMaxSize { get; set; }
        public int DvdMaxSize { get; set; }
        public int HdtvMaxSize { get; set; }
        public int Webdl720pMaxSize { get; set; }
        public int Webdl1080pMaxSize { get; set; }
        public int Bluray720pMaxSize { get; set; }
        public int Bluray1080pMaxSize { get; set; }
    }
}