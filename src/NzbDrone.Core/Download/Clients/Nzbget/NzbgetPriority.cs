namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public enum NzbgetPriority
    {
        VeryLow = -100,
        Low = -50,
        Normal = 0,
        High = 50,
        VeryHigh = 100,
        Force = 900
    }
}
