using System.Xml;

namespace NzbDrone.Core.Providers.Core
{
    public interface IHttpProvider
    {
        string DownloadString(string request);
        string DownloadString(string request, string username, string password);
        void DownloadFile(string request, string filename);
        void DownloadFile(string request, string filename, string username, string password);
        XmlReader DownloadXml(string url);
    }
}