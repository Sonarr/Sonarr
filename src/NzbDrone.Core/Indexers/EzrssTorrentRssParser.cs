using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Exceptions;

namespace NzbDrone.Core.Indexers
{
    public class EzrssTorrentRssParser : TorrentRssParser
    {
        public EzrssTorrentRssParser()
        {
            UseGuidInfoUrl = true;
            UseEnclosureLength = false;
            UseEnclosureUrl = true;
        }

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            var document = LoadXmlDocument(indexerResponse);
            var items = GetItems(document).ToList();

            if (items.Count == 1 && GetTitle(items.First()).Equals("No items exist - Try again later"))
            {
                throw new IndexerException(indexerResponse, "No results were found");
            }

            return base.PreProcess(indexerResponse);
        }

        protected override long GetSize(XElement item)
        {
            var contentLength = item.FindDecendants("contentLength").SingleOrDefault();

            if (contentLength != null)
            {
                return (long)contentLength;
            }

            return base.GetSize(item);
        }

        protected override string GetInfoHash(XElement item)
        {
            var infoHash = item.FindDecendants("infoHash").SingleOrDefault();
            return (string)infoHash;
        }

        protected override string GetMagnetUrl(XElement item)
        {
            var magnetURI = item.FindDecendants("magnetURI").SingleOrDefault();
            return (string)magnetURI;
        }

        protected override int? GetSeeders(XElement item)
        {
            var seeds = item.FindDecendants("seeds").SingleOrDefault();
            if (seeds != null)
            {
                return (int)seeds;
            }

            return base.GetSeeders(item);
        }

        protected override int? GetPeers(XElement item)
        {
            var peers = item.FindDecendants("peers").SingleOrDefault();
            if (peers != null)
            {
                return (int)peers;
            }

            return base.GetPeers(item);
        }
    }
}
