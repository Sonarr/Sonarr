using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists
{
    public interface IParseImportListResponse
    {
        IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse);
    }
}
