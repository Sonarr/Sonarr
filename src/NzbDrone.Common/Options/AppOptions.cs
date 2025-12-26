namespace NzbDrone.Common.Options;

public class AppOptions
{
    public string InstanceName { get; set; }
    public string Theme { get; set; }
    public bool? LaunchBrowser { get; set; }
    public bool? ProfilerEnabled { get; set; }
    public string ProfilerPosition { get; set; }
}
