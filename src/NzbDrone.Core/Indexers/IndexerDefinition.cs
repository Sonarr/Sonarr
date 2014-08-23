using System;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public class IndexerDefinition : ProviderDefinition
    {
        public Boolean EnableRss { get; set; }
        public Boolean EnableSearch { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public Boolean SupportsRss { get; set; }
        public Boolean SupportsSearch { get; set; }

        public override Boolean Enable
        {
            get
            {
                return EnableRss || EnableSearch;
            }
        }
    }
}
