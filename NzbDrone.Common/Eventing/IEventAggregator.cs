using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Common.Eventing
{
    /// <summary>
    ///   Enables loosely-coupled publication of events.
    /// </summary>
    public interface IEventAggregator
    {
        void Publish<TEvent>(TEvent message) where TEvent : IEvent;
    }
}