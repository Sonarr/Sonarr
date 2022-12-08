using System;
using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportList : IProvider
    {
        ImportListType ListType { get; }
        TimeSpan MinRefreshInterval { get; }
        IList<ImportListItemInfo> Fetch();
    }
}
