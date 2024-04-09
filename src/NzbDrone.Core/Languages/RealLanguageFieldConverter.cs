using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Languages
{
    public class RealLanguageFieldConverter : ISelectOptionsConverter
    {
        public List<SelectOption> GetSelectOptions()
        {
            return Language.All
                .Where(l => l != Language.Unknown)
                .OrderBy(l => l.Id > 0).ThenBy(l => l.Name)
                .ToList()
                .ConvertAll(v => new SelectOption { Value = v.Id, Name = v.Name });
        }
    }
}
