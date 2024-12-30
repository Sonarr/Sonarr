using Workarr.Download;

namespace Workarr.Housekeeping.Housekeepers
{
    public class FixFutureDownloadClientStatusTimes : FixFutureProviderStatusTimes<DownloadClientStatus>, IHousekeepingTask
    {
        public FixFutureDownloadClientStatusTimes(IDownloadClientStatusRepository downloadClientStatusRepository)
            : base(downloadClientStatusRepository)
        {
        }
    }
}
