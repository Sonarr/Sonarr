using Workarr.Messaging;

namespace Workarr.Tv.Events
{
    public class SeriesMovedEvent : IEvent
    {
        public Series Series { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public SeriesMovedEvent(Series series, string sourcePath, string destinationPath)
        {
            Series = series;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
