using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerParsingService
    {
        IEnumerable<ReleaseInfo> Parse(IIndexer indexer, string xml, string url);
    }

    public class IndexerParsingService : IIndexerParsingService
    {
        public IEnumerable<ReleaseInfo> Parse(IIndexer indexer, string xml, string url)
        {
            return indexer.Parser.Process(xml, url);
        }
    }
}
