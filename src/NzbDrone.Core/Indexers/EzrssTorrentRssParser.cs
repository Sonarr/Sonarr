using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public class EzrssTorrentRssParser : TorrentRssParser
    {
        public const String ns = "{http://xmlns.ezrss.it/0.1/}";
        
        public EzrssTorrentRssParser()
        {
            UseGuidInfoUrl = true;
            UseEnclosureLength = false;
            UseEnclosureUrl = true;
        }

        protected virtual XElement GetEzrssElement(XElement item, String name)
        {
            var element = item.Element(ns + name);

            if (element == null)
            {
                element = item.Element(ns + "torrent");
                if (element != null)
                {
                    element = element.Element(ns + name);
                }
            }

            return element;
        }

        protected override Int64 GetSize(XElement item)
        {
            var contentLength = GetEzrssElement(item, "contentLength");

            if (contentLength != null)
            {
                return (Int64)contentLength;
            }

            return base.GetSize(item);
        }

        protected override String GetInfoHash(XElement item)
        {
            var infoHash = GetEzrssElement(item, "infoHash");

            return (String)infoHash;
        }

        protected override String GetMagnetUrl(XElement item)
        {
            var magnetURI = GetEzrssElement(item, "magnetURI");

            return (String)magnetURI;
        }

        protected override Int32? GetSeeders(XElement item)
        {
            var seeds = GetEzrssElement(item, "seeds");

            if (seeds != null)
            {
                return (Int32)seeds;
            }

            return base.GetPeers(item);
        }

        protected override Int32? GetPeers(XElement item)
        {
            var peers = GetEzrssElement(item, "peers");

            if (peers != null)
            {
                return (Int32)peers;
            }

            return base.GetPeers(item);
        }
    }
}
