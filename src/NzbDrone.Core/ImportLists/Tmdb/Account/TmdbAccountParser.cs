using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public class TmdbAccountParser : TmdbParserBase<TmdbPagedResource<TmdbMediaResource>>
{
    protected override IEnumerable<ImportListItemInfo> ParseResponse(TmdbPagedResource<TmdbMediaResource> resource)
    {
        return resource.Results.Select(AsImportable);
    }
}
