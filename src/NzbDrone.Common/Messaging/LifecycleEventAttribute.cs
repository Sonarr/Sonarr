using System;

namespace NzbDrone.Common.Messaging
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class LifecycleEventAttribute : Attribute
    {
    }
}
