using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.YggTorrent
{
    public class YggTorrentHtmlParser : IParseIndexerResponse
    {
        private Logger _logger;

        private string[] XPathForItem { get; set; }
        private YggTorrentSettings Settings { get; set; }

        public YggTorrentHtmlParser(YggTorrentSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            
            _logger = NzbDroneLogger.GetLogger(this);
            Settings = settings;
            XPathForItem = Settings.XPathItem.Split(';');
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            List<ReleaseInfo> releases = new List<ReleaseInfo>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(indexerResponse.Content);
            HtmlNodeCollection node = doc.DocumentNode.SelectNodes(Settings.XPathItems);

            if (node == null)
            {
                return releases;
            }

            try
            {
                foreach (HtmlNode n in node)
                {
                    string id = GetNodeValue(n, XPathForItem[0]);
                    string name = GetNodeValue(n, XPathForItem[1]);
                    long timestamp = Convert.ToInt64(GetNodeValue(n, XPathForItem[2]));
                    string size = GetNodeValue(n, XPathForItem[3]);
                    int seeders = Convert.ToInt32(GetNodeValue(n, XPathForItem[4]));
                    int leechers = Convert.ToInt32(GetNodeValue(n, XPathForItem[5]));

                    ReleaseInfo releaseInfo = new ReleaseInfo();
                    releaseInfo.Guid = String.Format("TARGET-{0}", id);
                    releaseInfo.DownloadUrl = String.Format("{0}/{1}{2}", Settings.BaseUrl, Settings.DownloadUrlFormat, id).Trim();
                    releaseInfo.Title = name.Trim();
                    releaseInfo.PublishDate = GetAge(timestamp);
                    releaseInfo.Size = GetSize(size);
                    releaseInfo.Source = (seeders - leechers).ToString();

                    releases.Add(releaseInfo);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Parsing item release info failed. Actual XPath may not be up to date anymore.");
            }

            return releases;
        }

        private long GetSize(string size)
        {
            return Convert.ToInt64(size.ToByte());
        }

        private DateTime GetAge(long timestamp)
        {
            return new DateTime(1970, 1, 1).AddSeconds(timestamp);
        }

        private string GetNodeValue(HtmlNode n, string xPath)
        {
            var values = xPath.Split('|');
            var node = n.SelectSingleNode(values[0]);

            if (values.Length == 2)
                return node.GetAttributeValue(values[1], "");

            return node.InnerHtml;
        }
    }
}
