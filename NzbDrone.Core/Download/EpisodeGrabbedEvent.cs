using NzbDrone.Common.Messaging;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public class EpisodeGrabbedEvent : IEvent
    {
        public RemoteEpisode Episode { get; private set; }

        public EpisodeGrabbedEvent(RemoteEpisode episode)
        {
            Episode = episode;
        }
    }
}