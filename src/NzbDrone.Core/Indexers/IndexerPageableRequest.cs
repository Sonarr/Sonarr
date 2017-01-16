using System.Collections;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers
{
    public class IndexerPageableRequest : IEnumerable<IndexerRequest>
    {
        private readonly IEnumerable<IndexerRequest> _enumerable;

        public IndexerPageableRequest(IEnumerable<IndexerRequest> enumerable)
        {
            _enumerable = enumerable;
        }

        public IEnumerator<IndexerRequest> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }
    }
}
