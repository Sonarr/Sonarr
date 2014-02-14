using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Events
{
    public class EpisodeGrabbedEvent : IEvent
    {
        public RemoteEpisode Episode { get; private set; }
        public String DownloadClient { get; set; }
        public String DownloadClientId { get; set; }

        public EpisodeGrabbedEvent(RemoteEpisode episode)
        {
            Episode = episode;
        }
    }
}