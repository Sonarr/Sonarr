namespace NzbDrone.Core.Model.Sabnzbd
{
    public enum SabPriorityType
    {
        Default = -100,
        Paused = -2,
        Low = -1,
        Normal = 0,
        High = 1,
        Top = 2
    }
}