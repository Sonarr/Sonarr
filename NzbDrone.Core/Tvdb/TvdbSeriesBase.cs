using System;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace NzbDrone.Core.Tvdb
{
    [XmlRoot(ElementName = "Data")]
    public class TvdbSeriesRecordRoot
    {
        [XmlElement]
        public TvdbSeriesBase Series { get; set; }
    }

    public class TvdbSeriesBase
    {
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public string Actors { get; set; }

        [XmlElement]
        public string Airs_DayOfWeek { get; set; }

        [XmlElement]
        public string Airs_Time { get; set; }

        [XmlElement]
        public string ContentRating { get; set; }

        [XmlElement(ElementName = "FirstAired")]
        public string FirstAiredString
        {
            get { return FirstAired.HasValue ? FirstAired.Value.ToString("yyyy-MM-dd") : null; }
            set
            {
                DateTime d;
                if(DateTime.TryParse(value, out d))
                    FirstAired = d;
                else
                    FirstAired = null;
            }
        }

        [XmlIgnore]
        public DateTime? FirstAired { get; set; }

        [XmlElement]
        public string Genre { get; set; }

        [XmlElement]
        public string IMDB_ID { get; set; }

        [XmlElement]
        public string Language { get; set; }

        [XmlElement]
        public string Network { get; set; }

        [XmlElement]
        public string Overview { get; set; }

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

        [XmlElement]
        public int? Runtime { get; set; }

        [XmlElement]
        public string SeriesIDString
        {
            get { return SeriesID.HasValue ? SeriesID.Value.ToString() : null; }
            set
            {
                int i;
                if(int.TryParse(value, out i))
                    SeriesID = i;
                else
                    SeriesID = null;
            }
        }

        [XmlIgnore]
        public int? SeriesID { get; set; }

        [XmlElement]
        public string SeriesName { get; set; }

        [XmlElement]
        public string Status { get; set; }

        [XmlElement]
        public string added { get; set; }

        [XmlElement]
        public string addedBy { get; set; }

        [XmlElement]
        public string banner { get; set; }

        [XmlElement]
        public string fanart { get; set; }

        [XmlElement]
        public Int64 lastupdated { get; set; }

        [XmlElement]
        public string poster { get; set; }

        [XmlElement]
        public string zap2it_id { get; set; }
    }
}
