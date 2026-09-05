using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public class TmdbDiscoverParser : TmdbParserBase<TmdbPagedResource<TmdbMediaResource>>
{
    protected override IEnumerable<ImportListItemInfo> ParseResponse(TmdbPagedResource<TmdbMediaResource> resource)
    {
        return resource.Results.Select(AsImportable);
    }
}
