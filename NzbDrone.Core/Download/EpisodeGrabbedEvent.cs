using NzbDrone.Common.Eventing;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Download
{
    public class EpisodeGrabbedEvent : IEvent
    {
        public EpisodeParseResult ParseResult { get; private set; }

        public EpisodeGrabbedEvent(EpisodeParseResult parseResult)
        {
            ParseResult = parseResult;
        }
    }
}