using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;

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

        protected override Int64 GetSize(XElement item)
        {
            var contentLength = item.FindDecendants("contentLength").SingleOrDefault();

            if (contentLength != null)
            {
                return (Int64)contentLength;
            }

            return base.GetSize(item);
        }

        protected override String GetInfoHash(XElement item)
        {
            var infoHash = item.FindDecendants("infoHash").SingleOrDefault();
            return (String)infoHash;
        }

        protected override String GetMagnetUrl(XElement item)
        {
            var magnetURI = item.FindDecendants("magnetURI").SingleOrDefault();
            return (String)magnetURI;
        }

        protected override Int32? GetSeeders(XElement item)
        {
            var seeds = item.FindDecendants("seeds").SingleOrDefault();
            if (seeds != null)
            {
                return (Int32)seeds;
            }

            return base.GetPeers(item);
        }

        protected override Int32? GetPeers(XElement item)
        {
            var peers = item.FindDecendants("peers").SingleOrDefault();
            if (peers != null)
            {
                return (Int32)peers;
            }

            return base.GetPeers(item);
        }
    }
}
