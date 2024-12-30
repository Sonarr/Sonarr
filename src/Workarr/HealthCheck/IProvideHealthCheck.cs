namespace Workarr.HealthCheck
{
    public interface IProvideHealthCheck
    {
        HealthCheck Check();
        bool CheckOnStartup { get; }
        bool CheckOnSchedule { get; }
    }
}
