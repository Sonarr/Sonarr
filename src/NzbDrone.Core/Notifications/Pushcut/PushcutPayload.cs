namespace NzbDrone.Core.Notifications.Pushcut
{
    public class PushcutPayload
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public bool? IsTimeSensitive { get; set; }
    }
}
