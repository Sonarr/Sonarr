namespace NzbDrone.Core.Indexers.AirDCPP.Responses
{
    public class DownloadResultResponse
    {
        public OneOf Oneof { get; set; }
    }

    public class OneOf
    {
        public Bundle_Info bundle_info { get; set; }
        public Directory_Downloads[] directory_downloads { get; set; }
    }

    public class Bundle_Info
    {
        public long id { get; set; }
        public bool merged { get; set; }
    }

    public class Directory_Downloads
    {
        public string target_name { get; set; }
        public string target_directory { get; set; }
        public long priority { get; set; }
        public long id { get; set; }
        public string list_path { get; set; }
        public User user { get; set; }
    }
}
