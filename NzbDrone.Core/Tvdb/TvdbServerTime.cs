using System;
using System.Linq;
using System.Xml.Serialization;

namespace NzbDrone.Core.Tvdb
{
    [XmlRoot(ElementName = "Items")]
    public class TvdbServerTime
    {
        [XmlElement]
        public Int64 Time { get; set; }
    }
}
