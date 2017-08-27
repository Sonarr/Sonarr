using NzbDrone.Core.Download;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class FixFutureDownloadClientStatusTimes : FixFutureProviderStatusTimes<DownloadClientStatus>, IHousekeepingTask
    {
        public FixFutureDownloadClientStatusTimes(IDownloadClientStatusRepository downloadClientStatusRepository)
            : base(downloadClientStatusRepository)
        {
        }
    }
}
