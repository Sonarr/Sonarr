using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeDownloadedEvent : IEvent
    {
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; private set; }
        public Series Series { get; set; }

        public EpisodeDownloadedEvent(ParsedEpisodeInfo parsedEpisodeInfo, Series series)
        {
            ParsedEpisodeInfo = parsedEpisodeInfo;
            Series = series;
        }
    }
}