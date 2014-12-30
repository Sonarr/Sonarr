using System;
using System.Collections.Generic;

namespace TVDBSharp.Models
{
    public class Updates : UnixTimestampedObject
    {
        public List<UpdatedBanner> UpdatedBanners { get; set; }
        public List<UpdatedEpisode> UpdatedEpisodes { get; set; }
        public List<UpdatedSerie> UpdatedSeries { get; set; }
    }

    public class UnixTimestampedObject
    {
        private static DateTime _startDate = new DateTime(1970, 1, 1);
        private int _unixTimestamp;

        public DateTime Timestamp
        {
            get { return _startDate.AddSeconds(_unixTimestamp); }
        }

        public int Time
        {
            set { _unixTimestamp = value; }
        }
    }

    public class UpdatedSerie : UnixTimestampedObject
    {
        public int Id { get; set; }
    }

    public class UpdatedEpisode : UnixTimestampedObject
    {
        public int Id { get; set; }
        public int SerieId { get; set; }
    }

    public class UpdatedBanner : UnixTimestampedObject
    {
        public int SerieId { get; set; }
        public string Format { get; set; }
        public string Language { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public int? SeasonNumber { get; set; }
    }
}