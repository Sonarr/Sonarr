namespace NzbDrone.Common.Http.Proxy
{
    public interface IHttpProxySettingsProvider
    {
        HttpProxySettings GetProxySettings(HttpRequest request);
    }
}
