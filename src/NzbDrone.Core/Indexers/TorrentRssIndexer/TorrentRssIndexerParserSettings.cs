using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
