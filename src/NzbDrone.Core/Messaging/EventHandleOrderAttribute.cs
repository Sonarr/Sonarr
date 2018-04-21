using System;

namespace NzbDrone.Core.Messaging
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandleOrderAttribute : Attribute
    {
        public EventHandleOrder EventHandleOrder { get; set; }

        public EventHandleOrderAttribute(EventHandleOrder eventHandleOrder)
        {
            EventHandleOrder = eventHandleOrder;
        }
    }

    public enum EventHandleOrder
    {
        First,
        Any,
        Last
    }
}
