using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeImportedEvent : IEvent
    {
        public LocalEpisode EpisodeInfo { get; private set; }
        public EpisodeFile ImportedEpisode { get; private set; }
        public Boolean NewDownload { get; set; }

        public EpisodeImportedEvent(LocalEpisode episodeInfo, EpisodeFile importedEpisode, bool newDownload)
        {
            EpisodeInfo = episodeInfo;
            ImportedEpisode = importedEpisode;
            NewDownload = newDownload;
        }
    }
}