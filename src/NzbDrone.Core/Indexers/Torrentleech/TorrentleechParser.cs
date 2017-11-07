using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Torrentleech
{
    public class TorrentleechParser : IParseIndexerResponse
    {
        private readonly TorrentleechSettings _settings;

        private readonly TorrentRssParser rssParser = new TorrentRssParser() { UseGuidInfoUrl = true, ParseSeedersInDescription = true };

        public TorrentleechParser(TorrentleechSettings settings)
        {
            _settings = settings;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            if (indexerResponse.Request.Url.ToString().Contains("rss.torrentleech.org"))
            {
                return rssParser.ParseResponse(indexerResponse);
            }

            var list = new List<ReleaseInfo>();

            var document = new HtmlDocument();
            document.LoadHtml(indexerResponse.Content);

            var table = document.GetElementbyId("torrenttable");

            var rows = table.Descendants("tr").ToList();

            if (rows.Count >= 2)
            {
                for (var i = 1; i < rows.Count; i++)
                {
                    var torrent = rows[i];

                    var name = FindColumnText(torrent, "name", true);
                    var downloadUrl = FindColumn(torrent, "quickdownload", true).GetAttributeValue("href", null);
                    var infoUrl = FindColumn(torrent, "name", true).GetAttributeValue("href", null);

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(downloadUrl))
                    {
                        continue;
                    }

                    int seeders;
                    int peers;

                    var sizeField = torrent.Elements("td").Where(e => e.GetAttributeValue("class", string.Empty).Contains("listcolumn")).ElementAt(1);
                    if (!Regex.IsMatch(sizeField.InnerText, @"\d+([,.]\d+)?\s*[KkMmGgTt]?[Bb]"))
                    {
                        continue;
                    }

                    var units = new[] { "B", "KB", "MB", "GB", "TB", "PB" };

                    var match = Regex.Match(sizeField.InnerText.ToUpperInvariant(), @"([\d+.]+)\s?(B|KB|MB|GB|TB|PB)?");

                    var size = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * Math.Pow(1024, units.ToList().IndexOf(match.Groups[2].Value));

                    list.Add(new TorrentInfo
                    {
                        Title = name,
                        DownloadUrl = downloadUrl,
                        InfoUrl = _settings.BaseUrl + infoUrl,
                        Seeders = int.TryParse(FindColumnText(torrent, "seeders"), out seeders) ? seeders : (int?)null,
                        Peers = int.TryParse(FindColumnText(torrent, "leechers"), out peers) ? peers : (int?)null,
                        Size = (long)size,
                        DownloadProtocol = DownloadProtocol.Torrent,
                    });
                }
            }
            return list;
        }

        private HtmlNode FindColumn(HtmlNode element, string className, bool contentOfAnchor = false)
        {
            var column = element.Elements("td").FirstOrDefault(td => td.GetAttributeValue("class", "").Contains(className));

            if (contentOfAnchor)
            {
                column = column?.Descendants("a").FirstOrDefault();
            }
            return column;
        }

        private string FindColumnText(HtmlNode element, string className, bool contentOfAnchor = false)
        {
            return FindColumn(element, className, contentOfAnchor)?.InnerText;
        }
    }
}
