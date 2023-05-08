using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.HealthCheck
{
    public interface IEventDrivenHealthCheck
    {
        IProvideHealthCheck HealthCheck { get; }

        bool ShouldExecute(IEvent message, bool previouslyFailed);
    }

    public class EventDrivenHealthCheck<TEvent> : IEventDrivenHealthCheck
    {
        public IProvideHealthCheck HealthCheck { get; set; }
        public CheckOnCondition Condition { get; set; }
        public ICheckOnCondition<TEvent> EventFilter { get; set; }

        public EventDrivenHealthCheck(IProvideHealthCheck healthCheck, CheckOnCondition condition)
        {
            HealthCheck = healthCheck;
            Condition = condition;
            EventFilter = healthCheck as ICheckOnCondition<TEvent>;
        }

        public bool ShouldExecute(IEvent message, bool previouslyFailed)
        {
            if (Condition == CheckOnCondition.SuccessfulOnly && previouslyFailed)
            {
                return false;
            }

            if (Condition == CheckOnCondition.FailedOnly && !previouslyFailed)
            {
                return false;
            }

            if (EventFilter != null && !EventFilter.ShouldCheckOnEvent((TEvent)message))
            {
                return false;
            }

            return true;
        }
    }
}
