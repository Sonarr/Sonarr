using System;
using System.IO;
using System.Linq;
using System.Xml;
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
            using (var xmlTextReader = XmlReader.Create(new StringReader(indexerResponse.Content), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreComments = true }))
            {
                var document = XDocument.Load(xmlTextReader);
                var items = GetItems(document).ToList();

                if (items.Count == 1 && GetTitle(items.First()).Equals("No items exist - Try again later"))
                {
                    throw new IndexerException(indexerResponse, "No results were found");
                }
            }

            return base.PreProcess(indexerResponse);
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

            return base.GetSeeders(item);
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
