namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public class ExpectedVersion
    {
        public int Version { get; set; }
        public IDiskStationProxy Proxy { get; set; }
    }
}
