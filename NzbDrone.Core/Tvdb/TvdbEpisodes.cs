using System;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace NzbDrone.Core.Tvdb
{
    [XmlRoot(ElementName = "Data")]
    public class TvdbEpisodeRoot
    {
        [XmlElement]
        public TvdbEpisode Episode { get; set; }
    }

    public class TvdbEpisode
    {
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public string Combined_episodenumber { get; set; }

        [XmlElement]
        public string Combined_season { get; set; }

        [XmlElement]
        public string DVD_chapter { get; set; }

        [XmlElement]
        public string DVD_discid { get; set; }

        [XmlElement]
        public string DVD_episodenumber { get; set; }

        [XmlElement]
        public string DVD_season { get; set; }

        [XmlElement]
        public string Director { get; set; }

        [XmlElement(ElementName = "EpImgFlag")]
        public string EpImgFlagString
        {
            get { return EpImgFlag.HasValue ? EpImgFlag.Value.ToString() : null; }
            set
            {
                int i;
                if(int.TryParse(value, out i))
                    EpImgFlag = i;
                else
                    EpImgFlag = null;
            }
        }

        [XmlIgnore]
        public int? EpImgFlag { get; set; }

        [XmlElement]
        public string EpisodeName { get; set; }

        [XmlElement]
        public int EpisodeNumber { get; set; }

        [XmlElement]
        public DateTime FirstAired { get; set; }

        [XmlElement]
        public string GuestStars { get; set; }

        [XmlElement]
        public string IMDB_ID { get; set; }

        [XmlElement]
        public string Language { get; set; }

        [XmlElement]
        public string Overview { get; set; }

        [XmlElement]
        public string ProductionCode { get; set; }

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
        public int SeasonNumber { get; set; }

        [XmlElement]
        public string Writer { get; set; }

        [XmlElement]
        public string absolute_number { get; set; }

        [XmlElement]
        public string airsafter_season { get; set; }

        [XmlElement]
        public string airsbefore_episode { get; set; }

        [XmlElement]
        public string airsbefore_season { get; set; }

        [XmlElement]
        public string filename { get; set; }

        [XmlElement]
        public Int64 lastupdated { get; set; }

        [XmlElement]
        public int? seasonid { get; set; }

        [XmlElement]
        public int seriesid { get; set; }
    }
}
