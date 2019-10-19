namespace NzbDrone.Core.HealthCheck
{
    public interface ICheckOnCondition<TEvent>
    {
        bool ShouldCheckOnEvent(TEvent message);
    }
}
