using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class AddSeriesModel
    {
        [DataType(DataType.Text)]
        [DisplayName("Single Series Path")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SingleSeries { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Series Root Path")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SeriesRoot { get; set; }
    }
}