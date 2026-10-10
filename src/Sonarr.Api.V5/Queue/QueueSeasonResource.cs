namespace Sonarr.Api.V5.Queue
{
    public class QueueSeasonResource
    {
        public int SeasonNumber { get; set; }
        public int EpisodeCount { get; set; }
        public int EpisodesWithFilesCount { get; set; }
    }
}
