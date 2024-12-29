namespace Workarr.Http.Proxy
{
    public interface IHttpProxySettingsProvider
    {
        HttpProxySettings GetProxySettings(HttpUri uri);
        HttpProxySettings GetProxySettings();
    }
}
