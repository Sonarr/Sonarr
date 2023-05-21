namespace NzbDrone.Core.Notifications.Signal
{
    public class SignalPayload
    {
        public string Message { get; set; }
        public string Number { get; set; }
        public string[] Recipients { get; set; }
    }
}
