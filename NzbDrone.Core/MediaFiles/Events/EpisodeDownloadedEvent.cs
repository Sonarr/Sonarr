using NzbDrone.Common.Eventing;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeDownloadedEvent : IEvent
    {
        public ParsedEpisodeInfo ParseResult { get; private set; }
        public Series Series { get; set; }

        public EpisodeDownloadedEvent(ParsedEpisodeInfo parseResult, Series series)
        {
            ParseResult = parseResult;
            Series = series;
        }
    }
}