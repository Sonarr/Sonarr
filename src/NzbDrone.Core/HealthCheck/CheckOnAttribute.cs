using System;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.HealthCheck
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CheckOnAttribute: Attribute
    {
        public Type EventType { get; set; }

        public CheckOnAttribute(Type eventType)
        {
            EventType = eventType;
        }
    }
}
