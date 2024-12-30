using Workarr.ThingiProvider;

namespace Workarr.ImportLists
{
    public interface IImportList : IProvider
    {
        ImportListType ListType { get; }
        TimeSpan MinRefreshInterval { get; }
        ImportListFetchResult Fetch();
    }
}
