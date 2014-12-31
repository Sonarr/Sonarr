using System.IO;
using System.Net;
using System.Xml.Linq;
using TVDBSharp.Models.Enums;

namespace TVDBSharp.Models.DAO
{
    /// <summary>
    ///     Standard implementation of the <see cref="IDataProvider" /> interface.
    /// </summary>
    public class DataProvider : IDataProvider
    {
        public string ApiKey { get; set; }
        private const string BaseUrl = "http://thetvdb.com";

        public XDocument GetShow(int showID)
        {
            return GetXDocumentFromUrl(string.Format("{0}/api/{1}/series/{2}/all/", BaseUrl, ApiKey, showID));
        }

        public XDocument GetEpisode(int episodeId, string lang)
        {
            return GetXDocumentFromUrl(string.Format("{0}/api/{1}/episodes/{2}/{3}.xml", BaseUrl, ApiKey, episodeId, lang));
        }

        public XDocument GetUpdates(Interval interval)
        {
            return GetXDocumentFromUrl(string.Format("{0}/api/{1}/updates/updates_{2}.xml", BaseUrl, ApiKey, IntervalHelpers.Print(interval)));
        }

        public XDocument Search(string query)
        {
            return GetXDocumentFromUrl(string.Format("{0}/api/GetSeries.php?seriesname={1}", BaseUrl, query));
        }

        private static XDocument GetXDocumentFromUrl(string url)
        {
            using (var web = new WebClient())
                using (var memoryStream = new MemoryStream(web.DownloadData(url)))
                    return XDocument.Load(memoryStream);
        }
    }
}