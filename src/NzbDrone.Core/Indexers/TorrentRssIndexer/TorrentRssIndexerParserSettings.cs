using System;

namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public class TorrentRssIndexerParserSettings
    {
        public Boolean UseEZTVFormat { get; set; }

        public Boolean ParseSeedersInDescription { get; set; }

        public Boolean ParseSizeInDescription { get; set; }

        public String SizeElementName { get; set; }
    }
}
