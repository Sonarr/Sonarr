using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Series;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeFileDeletedEvent : IEvent
    {
        public EpisodeFile EpisodeFile { get; private set; }
        public DeleteMediaFileReason Reason { get; private set; }

        public EpisodeFileDeletedEvent(EpisodeFile episodeFile, DeleteMediaFileReason reason)
        {
            EpisodeFile = episodeFile;
            Reason = reason;
        }
    }
}