using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NzbDrone.Core.Tvdb
{
    [XmlRoot(ElementName = "Mirrors")]
    public class TvdbMirrors
    {
        [XmlElement(ElementName = "Mirror")]
        public List<TvdbMirror> Mirrors { get; set; }
    }

    public class TvdbMirror
    {
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public string mirrorpath { get; set; }

        [XmlElement]
        public int typemask { get; set; }

        public bool IsXMLMirror
        {
            get { return (typemask & 1) != 0; }
        }

        public bool IsBannerMirror
        {
            get { return (typemask & 2) != 0; }
        }

        public bool IsZipMirror
        {
            get { return (typemask & 4) != 0; }
        }
    }
}
