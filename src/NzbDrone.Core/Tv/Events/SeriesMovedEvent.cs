using System;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesMovedEvent : IEvent
    {
        public Series Series { get; set; }
        public String SourcePath { get; set; }
        public String DestinationPath { get; set; }

        public SeriesMovedEvent(Series series, string sourcePath, string destinationPath)
        {
            Series = series;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
