using System;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportList : IProvider
    {
        ImportListType ListType { get; }
        TimeSpan MinRefreshInterval { get; }
        ImportListFetchResult Fetch();
    }
}
