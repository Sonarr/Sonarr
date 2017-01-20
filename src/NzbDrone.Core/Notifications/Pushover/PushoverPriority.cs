namespace NzbDrone.Core.Notifications.Pushover
{
    public enum PushoverPriority
    {
        Silent = -2,
        Quiet = -1,
        Normal = 0,
        High = 1,
        Emergency = 2
    }
}
