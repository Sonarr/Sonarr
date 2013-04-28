using System;

namespace NzbDrone.Core.Download
{
    public class QueueItem
    {
        public decimal Size { get; set; }

        public string Title { get; set; }

        public decimal SizeLeft { get; set; }

        public string Id { get; set; }

        public TimeSpan Timeleft { get; set; }
    }
}
