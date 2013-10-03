using System;

namespace NzbDrone.Core.Download
{
    public class QueueItem
    {
        public string Id { get; set; }
        public decimal Size { get; set; }
        public string Title { get; set; }
        public decimal SizeLeft { get; set; }
        public TimeSpan Timeleft { get; set; }
    }
}
