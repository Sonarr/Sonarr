using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public class SeriesEditedService : IHandle<SeriesEditedEvent>
    {
        private readonly IDiskScanService _diskScanService;

        public SeriesEditedService(IDiskScanService diskScanService)
        {
            _diskScanService = diskScanService;
        }

        public void Handle(SeriesEditedEvent message)
        {
            if (message.Series.SeriesType != message.OldSeries.SeriesType)
            {
                _diskScanService.Scan(message.Series);
            }
        }
    }
}
