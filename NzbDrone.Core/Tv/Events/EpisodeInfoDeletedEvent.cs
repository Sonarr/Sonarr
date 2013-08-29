using System.Collections.Generic;
using System.Collections.ObjectModel;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class EpisodeInfoDeletedEvent : IEvent
    {
        public ReadOnlyCollection<Episode> Episodes { get; private set; }

        public EpisodeInfoDeletedEvent(IList<Episode> episodes)
        {
            Episodes = new ReadOnlyCollection<Episode>(episodes);
        }
    }
}