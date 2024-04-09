using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerSettings : IProviderConfig
    {
        string BaseUrl { get; set; }

        // TODO: Need to Create UI field for this and turn functionality back on per indexer.
        IEnumerable<int> MultiLanguages { get; set; }
    }
}
