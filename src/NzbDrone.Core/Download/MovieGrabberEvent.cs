using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public class MovieGrabbedEvent : IEvent
    {
        public RemoteMovie Movie { get; private set; }
        public String DownloadClient { get; set; }
        public String DownloadId { get; set; }

        public MovieGrabbedEvent(RemoteMovie movie)
        {
            Movie = movie;
        }
    }
}