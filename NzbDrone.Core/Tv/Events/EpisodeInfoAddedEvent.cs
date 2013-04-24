using System.Collections.Generic;
using System.Collections.ObjectModel;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class EpisodeInfoAddedEvent : IEvent
    {
        public ReadOnlyCollection<Episode> Episodes { get; private set; }

        public EpisodeInfoAddedEvent(IList<Episode> episodes)
        {
            Episodes = new ReadOnlyCollection<Episode>(episodes);
        }
    }
}