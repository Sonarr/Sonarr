using System.Collections;

namespace Workarr.ImportLists
{
    public class ImportListPageableRequest : IEnumerable<ImportListRequest>
    {
        private readonly IEnumerable<ImportListRequest> _enumerable;

        public ImportListPageableRequest(IEnumerable<ImportListRequest> enumerable)
        {
            _enumerable = enumerable;
        }

        public IEnumerator<ImportListRequest> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }
    }
}
