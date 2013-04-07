using NzbDrone.Common.Eventing;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeDownloadedEvent : IEvent
    {
        public EpisodeParseResult ParseResult { get; private set; }

        public EpisodeDownloadedEvent(EpisodeParseResult parseResult)
        {
            ParseResult = parseResult;
        }
    }
}