using System.Collections.Generic;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads
{
    public class ExternalSeriesPayload
    {
        public int Id { get; set; }
        public int TvdbId { get; set; }
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Status { get; set; }
        public string SeriesType { get; set; }
        public string Network { get; set; }
        public int Runtime { get; set; }
        public Language OriginalLanguage { get; set; }
        public string Certification { get; set; }
        public HashSet<int> Tags { get; set; }
        public int QualityProfileId { get; set; }
    }
}
