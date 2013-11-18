using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeDownloadedEvent : IEvent
    {
        public LocalEpisode Episode { get; private set; }
        public EpisodeFile EpisodeFile { get; private set; }
        public List<EpisodeFile> OldFiles { get; private set; }

        public EpisodeDownloadedEvent(LocalEpisode episode, EpisodeFile episodeFile, List<EpisodeFile> oldFiles)
        {
            Episode = episode;
            EpisodeFile = episodeFile;
            OldFiles = oldFiles;
        }
    }
}