namespace NzbDrone.Core.Providers.Core
{
    public interface IHttpProvider
    {
        string DownloadString(string request);
        string DownloadString(string request, string username, string password);
        bool DownloadFile(string request, string filename);
        bool DownloadFile(string request, string filename, string username, string password);
    }
}