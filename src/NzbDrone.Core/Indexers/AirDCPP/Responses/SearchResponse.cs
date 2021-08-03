namespace NzbDrone.Core.Indexers.AirDCPP.Responses
{
    public class SearchResponse
    {
        public int id { get; set; }
        public int expires_in { get; set; }
        public string current_search_id { get; set; }
        public string queue_time { get; set; }
        public string queued_count { get; set; }
        public string result_count { get; set; }
        public string searches_sent_ago { get; set; }
    }
}
