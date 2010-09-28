namespace NzbDrone.Core.Providers
{
    public interface IHttpProvider
    {
        string GetRequest(string request);
    }
}
