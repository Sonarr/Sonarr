using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles.Series;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeFolderCreatedEvent : IEvent
    {
        public Tv.Series Series { get; private set; }
        public EpisodeFile EpisodeFile { get; private set; }
        public string SeriesFolder { get; set; }
        public string SeasonFolder { get; set; }
        public string EpisodeFolder { get; set; }

        public EpisodeFolderCreatedEvent(Tv.Series series, EpisodeFile episodeFile)
        {
            Series = series;
            EpisodeFile = episodeFile;
        }
    }
}
