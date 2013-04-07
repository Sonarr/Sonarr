using NzbDrone.Common.Eventing;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Download
{
    public class EpisodeGrabbedEvent : IEvent
    {
        public IndexerParseResult ParseResult { get; private set; }

        public EpisodeGrabbedEvent(IndexerParseResult parseResult)
        {
            ParseResult = parseResult;
        }
    }
}