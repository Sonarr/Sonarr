using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace NzbDrone.Core.Tvdb
{
    [XmlRoot(ElementName = "Banners")]
    public class TvdbBannerRoot
    {
        public TvdbBannerRoot()
        {
            Banners = new List<TvdbBanner>();
        }

        [XmlElement(ElementName = "Banner")]
        public List<TvdbBanner> Banners { get; set; }
    }

    public class TvdbBanner
    {
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public string BannerPath { get; set; }

        [XmlElement]
        public string BannerType { get; set; }

        [XmlElement]
        public string BannerType2 { get; set; }

        [XmlElement]
        public string Colors { get; set; }

        [XmlElement]
        public string Language { get; set; }

        [XmlElement("Rating")]
        public string RatingString
        {
            get { return Rating.HasValue ? Rating.Value.ToString() : null; }
            set
            {
                double d;
                if(double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                    Rating = d;
                else
                    Rating = null;
            }
        }

        [XmlIgnore]
        public double? Rating { get; set; }

        [XmlElement]
        public int? RatingCount { get; set; }

        [XmlElement(ElementName = "SeriesName")]
        public string SeriesNameString
        {
            get { return SeriesName.HasValue ? SeriesName.Value.ToString() : null; }
            set
            {
                bool b;
                if(bool.TryParse(value, out b))
                    SeriesName = b;
                else
                    SeriesName = null;
            }
        }

        [XmlIgnore]
        public bool? SeriesName { get; set; }

        [XmlElement]
        public string ThumbnailPath { get; set; }

        [XmlElement]
        public string VignettePath { get; set; }

        [XmlElement]
        public string Season { get; set; }
    }
}
