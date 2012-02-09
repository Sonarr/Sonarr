using System;
using NzbDrone.Core.Model;

namespace NzbDrone.Web.Models
{
    public class HistoryModel
    {
        public int HistoryId { get; set; }
        public int SeriesId { get; set; }
        public string SeriesTitle { get; set; }
        public string EpisodeNumbering { get; set; }
        public string EpisodeTitle { get; set; }
        public string EpisodeOverview { get; set; }
        public string NzbTitle { get; set; }
        public string Quality { get; set; }
        public string Date { get; set; }
        public bool IsProper { get; set; }
        public string Indexer { get; set; }
        public int EpisodeId { get; set; }
        public string Details { get; set; }
    }
}