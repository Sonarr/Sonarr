using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListPageableRequestChain
    {
        private List<List<ImportListPageableRequest>> _chains;

        public ImportListPageableRequestChain()
        {
            _chains = new List<List<ImportListPageableRequest>>();
            _chains.Add(new List<ImportListPageableRequest>());
        }

        public int Tiers => _chains.Count;

        public IEnumerable<ImportListPageableRequest> GetAllTiers()
        {
            return _chains.SelectMany(v => v);
        }

        public IEnumerable<ImportListPageableRequest> GetTier(int index)
        {
            return _chains[index];
        }

        public void Add(IEnumerable<ImportListRequest> request)
        {
            if (request == null)
            {
                return;
            }

            _chains.Last().Add(new ImportListPageableRequest(request));
        }

        public void AddTier(IEnumerable<ImportListRequest> request)
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

            _chains.Add(new List<ImportListPageableRequest>());
        }
    }
}
