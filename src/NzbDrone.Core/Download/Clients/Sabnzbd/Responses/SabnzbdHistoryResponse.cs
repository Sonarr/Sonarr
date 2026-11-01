namespace NzbDrone.Core.Download.Clients.Sabnzbd.Responses
{
    public class SabnzbdHistoryResponse : SabnzbdJsonError
    {
        public SabnzbdHistory History { get; set; }
    }
}
