namespace NzbDrone.Core.Providers
{
    public interface IDownloadProvider
    {
        bool AddByUrl(string url, string title); //Should accept something other than string (NzbInfo?) returns success or failure
        bool IsInQueue(string title); //Should accept something other than string (Episode?) returns bool
    }
}