using NzbDrone.Common.Eventing;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class EpisodeDownloadedEvent : IEvent
    {
        public FileNameParseResult ParseResult { get; private set; }

        public EpisodeDownloadedEvent(FileNameParseResult parseResult)
        {
            ParseResult = parseResult;
        }
    }
}