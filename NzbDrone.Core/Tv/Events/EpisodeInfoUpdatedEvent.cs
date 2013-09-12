using System.Collections.Generic;
using System.Collections.ObjectModel;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class EpisodeInfoUpdatedEvent : IEvent
    {
        public ReadOnlyCollection<Episode> Episodes { get; private set; }

        public EpisodeInfoUpdatedEvent(IList<Episode> episodes)
        {
            Episodes = new ReadOnlyCollection<Episode>(episodes);
        }
    }
}