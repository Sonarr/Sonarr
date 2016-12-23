namespace NzbDrone.Core.Notifications.Pushalot
{
    public class PushalotResponse
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
    }
}
