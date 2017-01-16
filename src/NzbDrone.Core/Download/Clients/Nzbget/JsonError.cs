namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class JsonError
    {
        public string Version { get; set; }
        public ErrorModel Error { get; set; }
    }
}
