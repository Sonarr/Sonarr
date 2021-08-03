namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public enum SabnzbdPriority
    {
        Default = -100,
        Paused = -2,
        Low = -1,
        Normal = 0,
        High = 1,
        Force = 2
    }
}
