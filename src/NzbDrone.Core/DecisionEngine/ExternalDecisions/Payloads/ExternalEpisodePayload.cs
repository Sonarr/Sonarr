using System;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads
{
    public class ExternalEpisodePayload
    {
        public int Id { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public int Runtime { get; set; }
        public bool HasFile { get; set; }
    }
}
