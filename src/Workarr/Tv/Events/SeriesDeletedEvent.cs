using Workarr.Messaging;

namespace Workarr.Tv.Events
{
    public class SeriesDeletedEvent : IEvent
    {
        public List<Series> Series { get; private set; }
        public bool DeleteFiles { get; private set; }
        public bool AddImportListExclusion { get; private set; }

        public SeriesDeletedEvent(List<Series> series, bool deleteFiles, bool addImportListExclusion)
        {
            Series = series;
            DeleteFiles = deleteFiles;
            AddImportListExclusion = addImportListExclusion;
        }
    }
}
