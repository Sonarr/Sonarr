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
            SeedsElementName = "seeds";
            InfoHashElementName = "infoHash";
            SizeElementName = "contentLength";
            MagnetElementName = "magnetURI";
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
    }
}
