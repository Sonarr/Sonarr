using System.Collections.Generic;

namespace NzbDrone.Core.Annotations
{
    public interface ISelectOptionsConverter
    {
        List<SelectOption> GetSelectOptions();
    }
}
