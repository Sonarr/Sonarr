namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IOperatingSystemVersionInfo
    {
        string Version { get; }
        string Name { get; }
        string FullName { get; }
    }
}