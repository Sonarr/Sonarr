namespace NzbDrone.Core.Download.Clients.Sabnzbd.Responses
{
    public class SabnzbdQueueResponse : SabnzbdJsonError
    {
        public SabnzbdQueue Queue { get; set; }
    }
}
