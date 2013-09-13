using System;
using System.Xml.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public class BasicTorrentRssParser : RssParserBase
    {
        protected override ReleaseInfo CreateNewReleaseInfo()
        {
            return new TorrentInfo();
        }

        protected override ReleaseInfo PostProcessor(XElement item, ReleaseInfo currentResult)
        {
            var torrentInfo = (TorrentInfo)currentResult;

            torrentInfo.MagnetUrl = MagnetUrl(item);
            torrentInfo.InfoHash = InfoHash(item);

            return torrentInfo;
        }

        protected override long GetSize(XElement item)
        {
            var elementLength =  GetTorrentElement(item).Element("contentLength");
            return Convert.ToInt64(elementLength.Value);
        }

        protected virtual string MagnetUrl(XElement item)
        {
            var elementLength = GetTorrentElement(item).Element("magnetURI");
            return elementLength.Value;
        }

        protected virtual string InfoHash(XElement item)
        {
            var elementLength = GetTorrentElement(item).Element("infoHash");
            return elementLength.Value;
        }

        private static XElement GetTorrentElement(XElement item)
        {
            return item.Element("torrent");
        }
    }
}