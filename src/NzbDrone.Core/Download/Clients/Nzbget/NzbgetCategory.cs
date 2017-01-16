namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetCategory
    {
        public string Name { get; set; }
        public string DestDir { get; set; }
        public bool Unpack { get; set; }
        public string DefScript { get; set; }
        public string Aliases { get; set; }
    }
}
