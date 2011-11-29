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
        public QualityTypes Cutoff { get; set; }

        [DisplayName("Allowed Qualities")]
        public List<QualityTypes> Allowed { get; set; }

        //Quality Booleans
        [DisplayName("SDTV")]
        public bool Sdtv { get; set; }

        [DisplayName("DVD")]
        public bool Dvd { get; set; }

        [DisplayName("HDTV")]
        public bool Hdtv { get; set; }

        [DisplayName("WEBDL")]
        public bool Webdl { get; set; }

        [DisplayName("Bluray720p")]
        public bool Bluray720p { get; set; }

        [DisplayName("Bluray1080p")]
        public bool Bluray1080p { get; set; }
    }
}