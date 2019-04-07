namespace NzbDrone.Core.Notifications.Gotify

{
    public enum GotifyPriority
    {
        Silent = -2,
        Quiet = -1,
        Normal = 0,
        High = 1,
        Emergency = 2
    }
}
