namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class TopWatcher
    {
        public int plays { get; set; }
        public string username { get; set; }
        public bool @protected { get; set; }
        public object full_name { get; set; }
        public object gender { get; set; }
        public string age { get; set; }
        public object location { get; set; }
        public object about { get; set; }
        public int joined { get; set; }
        public string avatar { get; set; }
        public string url { get; set; }
    }
}