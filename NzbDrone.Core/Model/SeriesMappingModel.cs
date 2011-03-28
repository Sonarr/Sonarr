using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model
{
    public class SeriesMappingModel
    {
        public string Path { get; set; }
        public int TvDbId { get; set; }
        public int QualityProfileId { get; set; }
    }
}
