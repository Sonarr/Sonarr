using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeImportedEvent:IEvent
    {
        public EpisodeFile EpisodeFile { get; private set; }

        public EpisodeImportedEvent(EpisodeFile episodeFile)
        {
            EpisodeFile = episodeFile;
        }
    }
}