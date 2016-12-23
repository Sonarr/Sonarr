namespace NzbDrone.Core.HealthCheck
{
    public interface IProvideHealthCheck
    {
        HealthCheck Check();
        bool CheckOnStartup { get; }
        bool CheckOnConfigChange { get; }
        bool CheckOnSchedule { get; }
    }
}
