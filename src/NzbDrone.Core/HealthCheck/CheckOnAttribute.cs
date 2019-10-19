using System;

namespace NzbDrone.Core.HealthCheck
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CheckOnAttribute : Attribute
    {
        public Type EventType { get; set; }
        public CheckOnCondition Condition { get; set; }

        public CheckOnAttribute(Type eventType, CheckOnCondition condition = CheckOnCondition.Always)
        {
            EventType = eventType;
            Condition = condition;
        }
    }

    public enum CheckOnCondition
    {
        Always,
        FailedOnly,
        SuccessfulOnly
    }
}
