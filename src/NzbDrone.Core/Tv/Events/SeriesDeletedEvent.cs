using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesDeletedEvent : IEvent
    {
        public Series Series { get; private set; }
        public bool DeleteFiles { get; private set; }
        public bool AddImportListExclusion { get; private set; }

        public SeriesDeletedEvent(Series series, bool deleteFiles, bool addImportListExclusion)
        {
            Series = series;
            DeleteFiles = deleteFiles;
            AddImportListExclusion = addImportListExclusion;
        }
    }
}
