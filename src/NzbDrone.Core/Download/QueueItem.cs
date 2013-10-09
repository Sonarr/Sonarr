using System;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public class QueueItem
    {
        public string Id { get; set; }
        public decimal Size { get; set; }
        public string Title { get; set; }
        public decimal Sizeleft { get; set; }
        public TimeSpan Timeleft { get; set; }
        public String Status { get; set; }
        public RemoteEpisode RemoteEpisode { get; set; }
    }
}
