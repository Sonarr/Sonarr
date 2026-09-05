using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public class TmdbPersonParser : TmdbParserBase<TmdbCreditsResource>
{
    private readonly TmdbPersonSettings _settings;

    public TmdbPersonParser(TmdbPersonSettings settings)
    {
        _settings = settings;
    }

    protected override IEnumerable<ImportListItemInfo> ParseResponse(TmdbCreditsResource resource)
    {
        var items = Enumerable.Empty<ImportListItemInfo>();

        var departments = _settings.IncludeDepartmentTypes
            .Cast<TmdbDepartmentType>()
            .Select(department => department switch
            {
                TmdbDepartmentType.CostumeMakeup => "Costume & Makeup",
                TmdbDepartmentType.VisualEffects => "Visual Effects",
                _ => department.ToString()
            }).ToHashSet();

        if (_settings.IncludingCastCredits && resource.Cast.Count > 0)
        {
            items = resource.Cast.Select(AsImportable);
        }

        if (departments.Count > 0 && resource.Crew.Count > 0)
        {
            items = items.Concat(resource.Crew
                .Where(c => departments.Contains(c.Department))
                .Select(AsImportable));
        }

        return items;
    }
}
