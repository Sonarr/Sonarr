using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface IDownloadProvider
    {
        bool AddByUrl(ItemInfo nzb); //Should accept something other than string (NzbInfo?) returns success or failure
        bool IsInQueue(Episode episode);//Should accept something other than string (Episode?) returns bool
    }
}
