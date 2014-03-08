namespace NzbDrone.Core.HealthCheck
{
    public interface IProvideHealthCheck
    {
        HealthCheck Check();
    }
}
