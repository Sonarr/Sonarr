using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Tmdb.List;

public class TmdbListParser : TmdbParserBase<TmdbPagedResource<TmdbMediaResource>>
{
    protected override IEnumerable<ImportListItemInfo> ParseResponse(TmdbPagedResource<TmdbMediaResource> resource)
    {
        return resource.Results.Select(AsImportable);
    }
}
