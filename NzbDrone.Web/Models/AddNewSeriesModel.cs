using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class AddNewSeriesModel
    {
        [Required(ErrorMessage = "Please enter a series name")]
        [DataType(DataType.Text)]
        [DisplayName("Single Series Path")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SeriesName { get; set; }
    }
}