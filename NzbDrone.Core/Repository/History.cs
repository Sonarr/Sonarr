using System;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [PrimaryKey("HistoryId")]
    public class History
    {
        public int HistoryId { get; set; }

        public int EpisodeId { get; set; }
        public int SeriesId { get; set; }
        public string NzbTitle { get; set; }
        public QualityTypes Quality { get; set; }
        public DateTime Date { get; set; }
        public bool IsProper { get; set; }
        public string Indexer { get; set; }
    }
}