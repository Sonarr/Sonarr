using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeImportedEvent : IEvent
    {
        public LocalEpisode DroppedEpisode { get; private set; }
        public EpisodeFile ImportedEpisode { get; private set; }

        public EpisodeImportedEvent(LocalEpisode droppedEpisode, EpisodeFile importedEpisode)
        {
            DroppedEpisode = droppedEpisode;
            ImportedEpisode = importedEpisode;
        }
    }
}