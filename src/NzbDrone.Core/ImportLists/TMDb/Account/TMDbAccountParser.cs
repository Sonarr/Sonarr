using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.TMDb.Account;

public class TMDbAccountParser : TMDbParserBase<TMDbPagedResource<TMDbMediaResource>>
{
    protected override IEnumerable<ImportListItemInfo> ParseResponse(TMDbPagedResource<TMDbMediaResource> resource)
    {
        return resource.Results.Select(AsImportable);
    }
}
