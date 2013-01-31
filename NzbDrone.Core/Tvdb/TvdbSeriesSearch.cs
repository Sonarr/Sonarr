using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NzbDrone.Core.Tvdb
{
    [XmlRoot(ElementName = "Data")]
    public class TvdbSeriesSearchRoot
    {
        public TvdbSeriesSearchRoot()
        {
            Series = new List<TvdbSeriesSearchItem>();
        }

        [XmlElement(ElementName = "Series")]
        public List<TvdbSeriesSearchItem> Series { get; set; }
    }
}
