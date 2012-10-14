using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Web.Models
{
    public class QualityProfileModel
    {
        public int QualityProfileId { get; set; }

        [Required(ErrorMessage = "A Name is Required")]
        [DisplayName("Name")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        [DisplayName("Cut-off")]
        [Required(ErrorMessage = "Valid Cut-off is Required")]
        public int Cutoff { get; set; }

        [DisplayName("Allowed Qualities")]
        public List<QualityTypes> Allowed { get; set; }

        //Quality Booleans
        [DisplayName("SDTV")]
        public bool Sdtv { get; set; }
        public int SdtvId { get; set; }

        [DisplayName("DVD")]
        public bool Dvd { get; set; }
        public int DvdId { get; set; }

        [DisplayName("HDTV")]
        public bool Hdtv { get; set; }
        public int HdtvId { get; set; }

        [DisplayName("WEBDL")]
        public bool Webdl { get; set; }
        public int WebdlId { get; set; }

        [DisplayName("Bluray720p")]
        public bool Bluray720p { get; set; }
        public int Bluray720pId { get; set; }

        [DisplayName("Bluray1080p")]
        public bool Bluray1080p { get; set; }
        public int Bluray1080pId { get; set; }
    }
}