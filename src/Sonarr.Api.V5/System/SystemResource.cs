using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Update;

namespace Sonarr.Api.V5.System;

public class SystemResource
{
    public required string AppName { get; set; }
    public required string InstanceName { get; set; }
    public required string Version { get; set; }
    public DateTime BuildTime { get; set; }
    public bool IsDebug { get; set; }
    public bool IsProduction { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsUserInteractive { get; set; }
    public required string StartupPath { get; set; }
    public required string AppData { get; set; }
    public required string OsName { get; set; }
    public required string OsVersion { get; set; }
    public bool IsNetCore { get; set; }
    public bool IsLinux { get; set; }
    public bool IsOsx { get; set; }
    public bool IsWindows { get; set; }
    public bool IsDocker { get; set; }
    public RuntimeMode Mode { get; set; }
    public required string Branch { get; set; }
    public AuthenticationType Authentication { get; set; }
    public int MigrationVersion { get; set; }
    public required string UrlBase { get; set; }
    public required Version RuntimeVersion { get; set; }
    public required string RuntimeName { get; set; }
    public DateTime StartTime { get; set; }
    public required string PackageVersion { get; set; }
    public required string PackageAuthor { get; set; }
    public UpdateMechanism PackageUpdateMechanism { get; set; }
    public required string PackageUpdateMechanismMessage { get; set; }
    public required Version DatabaseVersion { get; set; }
    public DatabaseType DatabaseType { get; set; }
}
