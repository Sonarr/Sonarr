using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class TvDbSearchResultModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FirstAired { get; set; }
    }
}