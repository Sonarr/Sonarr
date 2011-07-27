using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NzbDrone.Web.Models
{
    public class ExistingSeriesModel
    {
        public SelectList Quality { get; set; }

        public List<Tuple<string, string>> ExistingSeries { get; set; }
    }
}