using System.Collections.Generic;
using Diacritical;

namespace NzbDrone.Common.Globalization;

public class AdditionalDiacriticsProvider : IDiacriticProvider
{
    public IDictionary<char, string> Provide()
    {
        return new Dictionary<char, string>
        {
            { 'ð', "d" },
            { 'Ð', "D" },
            { 'þ', "th" },
        };
    }
}
