using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NzbDrone.Core.Tvdb
{
    [XmlRoot(ElementName = "Items")]
    public class TvdbUpdateItems
    {
        public TvdbUpdateItems()
        {
            Series = new List<int>();
            Episodes = new List<int>();
        }

        [XmlElement]
        public Int64 Time { get; set; }

        [XmlElement(ElementName = "Series")]
        public List<int> Series { get; set; }

        [XmlElement(ElementName = "Episode")]
        public List<int> Episodes { get; set; }
    }
}
