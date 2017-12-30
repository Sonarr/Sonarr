namespace NzbDrone.Core.HealthCheck
{
    public class EventDrivenHealthCheck
    {
        public IProvideHealthCheck HealthCheck { get; set; }
        public CheckOnCondition Condition { get; set; }

        public EventDrivenHealthCheck(IProvideHealthCheck healthCheck, CheckOnCondition condition)
        {
            HealthCheck = healthCheck;
            Condition = condition;
        }
    }
}
