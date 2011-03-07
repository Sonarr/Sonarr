namespace NzbDrone.Core.Providers
{
    public interface IHttpProvider
    {
        string DownloadString(string request);
        string DownloadString(string request, string username, string password);
    }
}