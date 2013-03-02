using System;
using System.Collections.Generic;
using System.ComponentModel;
using NzbDrone.Core.Model;


namespace NzbDrone.Core.Repository.Search
{
    public class SearchHistory
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeId { get; set; }
        public DateTime SearchTime { get; set; }
        public bool SuccessfulDownload { get; set; }

        public List<SearchHistoryItem> SearchHistoryItems { get; set; }

        public List<int> Successes { get; set; }

        public string SeriesTitle { get; set; }

        public bool IsDaily { get; set; }

        public int? EpisodeNumber { get; set; }

        public string EpisodeTitle { get; set; }

        public DateTime AirDate { get; set; }

        public int TotalItems { get; set; }

        public int SuccessfulCount { get; set; }
    }
}