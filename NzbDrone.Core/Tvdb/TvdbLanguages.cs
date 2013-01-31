using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NzbDrone.Core.Tvdb
{
    [XmlRoot(ElementName = "Languages")]
    public class TvdbLanguagesRoot
    {
        public TvdbLanguagesRoot()
        {
            Languages = new List<TvdbLanguage>();
        }

        [XmlElement(ElementName = "Language")]
        public List<TvdbLanguage> Languages { get; set; }
    }

    public class TvdbLanguage
    {
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public string name { get; set; }

        [XmlElement]
        public string abbreviation { get; set; }
    }
}
