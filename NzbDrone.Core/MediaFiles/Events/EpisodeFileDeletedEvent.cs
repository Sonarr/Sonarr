using NzbDrone.Common.Eventing;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeFileDeletedEvent : IEvent
    {
        public EpisodeFile EpisodeFile { get; private set; }

        public EpisodeFileDeletedEvent(EpisodeFile episodeFile)
        {
            EpisodeFile = episodeFile;
        }
    }
}