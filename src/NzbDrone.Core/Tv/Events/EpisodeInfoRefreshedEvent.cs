using System.Collections.Generic;
using System.Collections.ObjectModel;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class EpisodeInfoRefreshedEvent : IEvent
    {
        public Series Series { get; set; }
        public ReadOnlyCollection<Episode> Added { get; private set; }
        public ReadOnlyCollection<Episode> Updated { get; private set; }
        public ReadOnlyCollection<Episode> Removed { get; private set; }

        public EpisodeInfoRefreshedEvent(Series series, IList<Episode> added, IList<Episode> updated, IList<Episode> removed)
        {
            Series = series;
            Added = new ReadOnlyCollection<Episode>(added);
            Removed = new ReadOnlyCollection<Episode>(removed);
        }
    }
}
