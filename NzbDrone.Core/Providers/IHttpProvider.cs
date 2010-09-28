namespace NzbDrone.Core.Providers
{
    public interface IHttpProvider
    {
        string DownloadString(string request);
    }
}
