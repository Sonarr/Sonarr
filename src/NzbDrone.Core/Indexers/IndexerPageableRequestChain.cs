using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Indexers
{
    public class IndexerPageableRequestChain
    {
        private List<List<IndexerPageableRequest>> _chains;

        public IndexerPageableRequestChain()
        {
            _chains = new List<List<IndexerPageableRequest>>();
            _chains.Add(new List<IndexerPageableRequest>());
        }

        public int Tiers => _chains.Count;

        public IEnumerable<IndexerPageableRequest> GetAllTiers()
        {
            return _chains.SelectMany(v => v);
        }

        public IEnumerable<IndexerPageableRequest> GetTier(int index)
        {
            return _chains[index];
        }

        public void Add(IEnumerable<IndexerRequest> request)
        {
            if (request == null)
            {
                return;
            }

            _chains.Last().Add(new IndexerPageableRequest(request));
        }

        public void AddToTier(int tierIndex, IEnumerable<IndexerRequest> request)
        {
            if (request == null)
            {
                return;
            }

            _chains[tierIndex].Add(new IndexerPageableRequest(request));
        }

        public void AddTier(IEnumerable<IndexerRequest> request)
        {
            AddTier();
            Add(request);
        }

        public void AddTier()
        {
            if (_chains.Last().Count == 0)
            {
                return;
            }

            _chains.Add(new List<IndexerPageableRequest>());
        }
    }
}
