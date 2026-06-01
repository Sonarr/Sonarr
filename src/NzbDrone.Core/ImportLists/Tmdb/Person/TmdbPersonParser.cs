using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public class TmdbPersonParser : TmdbParserBase<TmdbCreditsResource>
{
    private readonly bool _isIncludingCastCredits;
    private readonly HashSet<string> _departments;

    public TmdbPersonParser(TmdbPersonSettings settings)
    {
        _isIncludingCastCredits = settings.IncludingCastCredits;
        _departments = GetDepartmentNames(settings.IncludeDepartmentTypes);
    }

    protected override IEnumerable<ImportListItemInfo> ParseResponse(TmdbCreditsResource resource)
    {
        var items = Enumerable.Empty<ImportListItemInfo>();
        if (_isIncludingCastCredits && resource.Cast.Count > 0)
        {
            items = resource.Cast.Select(AsImportable);
        }

        if (_departments.Count > 0 && resource.Crew.Count > 0)
        {
            items = items.Concat(resource.Crew
                .Where(c => _departments.Contains(c.Department))
                .Select(AsImportable));
        }

        return items;
    }

    private static HashSet<string> GetDepartmentNames(IEnumerable<int> departmentTypes)
    {
        return departmentTypes
            .Cast<TmdbDepartmentType>()
            .Select(department => department switch
            {
                TmdbDepartmentType.CostumeMakeup => "Costume & Makeup",
                TmdbDepartmentType.VisualEffects => "Visual Effects",
                _ => department.ToString()
            }).ToHashSet();
    }
}
