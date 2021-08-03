namespace NzbDrone.Core.Indexers.AirDCPP.Responses
{
    public class SearchHubResponse
    {
        public int queue_time { get; set; }
        public long search_id { get; set; }
        public int queued_count { get; set; }
    }
}
