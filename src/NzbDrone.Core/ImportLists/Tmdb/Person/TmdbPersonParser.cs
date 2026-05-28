using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public class TmdbPersonParser : TmdbParserBase<TmdbCreditsResource>
{
    private readonly bool _isIncludingCastCredit;
    private readonly HashSet<string> _departments;

    public TmdbPersonParser(TmdbPersonSettings settings)
    {
        _isIncludingCastCredit = settings.IsIncludingCastCredit;
        _departments = GetWantedCrewDepartments(settings.IncludedCrewDepartmentCredits);
    }

    protected override IEnumerable<ImportListItemInfo> ParseResponse(TmdbCreditsResource resource)
    {
        var items = Enumerable.Empty<ImportListItemInfo>();
        if (_isIncludingCastCredit && resource.Cast.Count > 0)
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

    private static HashSet<string> GetWantedCrewDepartments(IEnumerable<int> includedCrewDepartmentCredits)
    {
        return includedCrewDepartmentCredits
            .Cast<TmdbCrewDepartment>()
            .Select(crewDepartment => crewDepartment switch
            {
                TmdbCrewDepartment.CostumeMakeup => "Costume & Makeup",
                TmdbCrewDepartment.VisualEffects => "Visual Effects",
                _ => crewDepartment.ToString()
            }).ToHashSet();
    }
}
