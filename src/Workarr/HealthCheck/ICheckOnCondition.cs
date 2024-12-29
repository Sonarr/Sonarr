namespace Workarr.HealthCheck
{
    public interface ICheckOnCondition<TEvent>
    {
        bool ShouldCheckOnEvent(TEvent message);
    }
}
