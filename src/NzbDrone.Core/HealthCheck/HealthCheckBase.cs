namespace NzbDrone.Core.HealthCheck
{
    public abstract class HealthCheckBase : IProvideHealthCheck
    {
        public abstract HealthCheck Check();

        public virtual bool CheckOnStartup => true;

        public virtual bool CheckOnConfigChange => true;

        public virtual bool CheckOnSchedule => true;
    }
}
