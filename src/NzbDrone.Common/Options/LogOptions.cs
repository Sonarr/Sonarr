namespace NzbDrone.Common.Options;

public class LogOptions
{
    public string Level { get; set; }
    public bool? FilterSentryEvents { get; set; }
    public int? Rotate { get; set; }
    public bool? Sql { get; set; }
    public string ConsoleLevel { get; set; }
    public bool? AnalyticsEnabled { get; set; }
    public string SyslogServer { get; set; }
    public int? SyslogPort { get; set; }
    public string SyslogLevel { get; set; }
}
