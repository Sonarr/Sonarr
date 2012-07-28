using System;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Model
{
    public class HistoryQueryModel
    {
        public int HistoryId { get; set; }
        public int EpisodeId { get; set; }
        public int SeriesId { get; set; }
        public string NzbTitle { get; set; }
        public QualityTypes Quality { get; set; }
        public DateTime Date { get; set; }
        public bool IsProper { get; set; }
        public string Indexer { get; set; }
        public string NzbInfoUrl { get; set; }

        public string EpisodeTitle { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string EpisodeOverview { get; set; }
        public string SeriesTitle { get; set; }public int Id { get; set; }
    }
}