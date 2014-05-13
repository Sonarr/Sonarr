using System;
using System.Xml.Linq;
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
            var torrentElement = GetTorrentElement(item);

            if (torrentElement != null)
            {
                torrentInfo.MagnetUrl = MagnetUrl(torrentElement);
                torrentInfo.InfoHash = InfoHash(torrentElement);
                torrentInfo.Seeds = Seeders(torrentElement);
                torrentInfo.Peers = Peers(torrentElement);
            }

            return torrentInfo;
        }

        protected override Int64 GetSize(XElement item)
        {
            var torrentElement = GetTorrentElement(item);

            if (torrentElement == null)
            {
                return 0;
            }

            var size = torrentElement.Element("contentLength");

            if (size == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt64(size.Value);
            }
        }

        protected virtual String MagnetUrl(XElement torrentElement)
        {
            var magnetLink = torrentElement.Element("magnetURI");

            if (magnetLink == null)
            {
                return null;
            }
            else
            {
                return magnetLink.Value;
            }
        }

        protected virtual String InfoHash(XElement torrentElement)
        {
            var infoHash = torrentElement.Element("infoHash");

            if (infoHash == null)
            {
                return null;
            }
            else
            {
                return infoHash.Value;
            }
        }

        protected virtual Int32? Seeders(XElement torrentElement)
        {
            var seeds = torrentElement.Element("seeds");

            if (seeds == null)
            {
                return null;
            }
            else
            {
                return Convert.ToInt32(seeds.Value);
            }
        }

        protected virtual Int32? Peers(XElement torrentElement)
        {
            var peers = torrentElement.Element("peers");

            if (peers == null)
            {
                return null;
            }
            else
            {
                return Convert.ToInt32(peers.Value);
            }
        }

        protected virtual XElement GetTorrentElement(XElement item)
        {
            return item.Element("torrent");
        }
    }
}