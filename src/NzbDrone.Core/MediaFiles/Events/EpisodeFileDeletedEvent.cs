using System;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeFileDeletedEvent : IEvent
    {
        public EpisodeFile EpisodeFile { get; private set; }
        public Boolean ForUpgrade { get; private set; }

        public EpisodeFileDeletedEvent(EpisodeFile episodeFile, Boolean forUpgrade)
        {
            EpisodeFile = episodeFile;
            ForUpgrade = forUpgrade;
        }
    }
}