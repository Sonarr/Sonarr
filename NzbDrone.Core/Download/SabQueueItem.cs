using System;
using Newtonsoft.Json;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Download.Clients.Sabnzbd.JsonConverters;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Download
{
    public class QueueItem
    {
        public decimal Size { get; set; }

        public string Title { get; set; }

        public decimal SizeLeft { get; set; }

        public int Percentage { get; set; }

        public string Id { get; set; }

        public TimeSpan Timeleft { get; set; }
    }
}
