namespace NzbDrone.Core.Indexers.AirDCPP.Responses
{
    public class SearchResultsResponse
    {
        public SearchResult[] SearchResults { get; set; }
    }

    public class SearchResult
    {
        public string id { get; set; }
        public string name { get; set; }
        public float relevance { get; set; }
        public long hits { get; set; }
        public Users users { get; set; }
        public Type type { get; set; }
        public string path { get; set; }
        public string tth { get; set; }
        public Dupe dupe { get; set; }
        public long? time { get; set; }
        public Slots slots { get; set; }
        public long connection { get; set; }
        public long size { get; set; }
    }

    public class Users
    {
        public long count { get; set; }
        public User user { get; set; }
    }

    public class User
    {
        public string cid { get; set; }
        public string hub_url { get; set; }
        public string nicks { get; set; }
        public string hub_names { get; set; }
        public string[] flags { get; set; }
    }

    public class Type
    {
        public string id { get; set; }
        public string str { get; set; }
        public long files { get; set; }
        public long directories { get; set; }
    }

    public class Dupe
    {
        public string id { get; set; }
        public string[] paths { get; set; }
    }

    public class Slots
    {
        public long free { get; set; }
        public long total { get; set; }
        public string str { get; set; }
    }
}
