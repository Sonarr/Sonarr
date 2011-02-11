namespace NzbDrone.Core.Model
{
    public enum SabnzbdPriorityType
    {
        Default = -100,
        Paused = -2,
        Low = -1,
        Normal = 0,
        High = 1,
        Top = 2
    }
}
