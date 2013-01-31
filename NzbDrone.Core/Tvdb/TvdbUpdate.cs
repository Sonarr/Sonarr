using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NzbDrone.Core.Tvdb
{
    public enum TvdbUpdatePeriod
    {
        day,
        week,
        month
    };

    [XmlRoot(ElementName = "Data")]
    public class TvdbUpdates
    {
        public TvdbUpdates()
        {
            Series = new List<TvdbUpdateSeries>();
        }

        [XmlAttribute]
        public Int64 time { get; set; }

        [XmlElement(ElementName = "Series")]
        public List<TvdbUpdateSeries> Series { get; set; }

        [XmlElement(ElementName = "Episode")]
        public List<TvdbUpdateEpisode> Episodes { get; set; }

        [XmlElement(ElementName = "Banner")]
        public List<TvdbUpdateBanner> Banners { get; set; }
    }

    public class TvdbUpdateSeries
    {
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public Int64 time { get; set; }
    }

    public class TvdbUpdateEpisode
    {
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public int Series { get; set; }

        [XmlElement]
        public Int64 time { get; set; }
    }

    public class TvdbUpdateBanner
    {
        /// <summary>
        ///     fanart, poster, season, series, episode, actors
        /// </summary>
        [XmlElement]
        public string type { get; set; }

        [XmlElement]
        public string format { get; set; }

        [XmlElement]
        public int Series { get; set; }

        /// <summary>
        ///     Only appears for season banners
        /// </summary>
        [XmlElement]
        public int? SeasonNum { get; set; }

        [XmlElement]
        public string language { get; set; }

        [XmlElement]
        public string path { get; set; }

        [XmlElement]
        public Int64 time { get; set; }
    }
}
