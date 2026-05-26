using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.TMDb.Discover;

public sealed class TMDbLanguageOptionsConverter : ISelectOptionsConverter
{
    public static string UnpackLanguage(int packedLanguage)
    {
        return string.Create(2,
            packedLanguage,
            static (span, v) =>
        {
            span[0] = (char)(v >> 8);
            span[1] = (char)(v & 0xFF);
        });
    }

    public List<SelectOption> GetSelectOptions()
    {
        var languageOptions = new List<SelectOption>(1)
        {
            new() { Name = "None", Value = 0 }
        };

        var neutralCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
        languageOptions.EnsureCapacity(neutralCultures.Length);

        var filteredLanguageOptions = neutralCultures
            .Where(c =>
                !c.Equals(CultureInfo.InvariantCulture) &&          // Where culture is not the default IV instance
                c.Parent.Equals(CultureInfo.InvariantCulture) &&    // Where culture is not a sub-culture of another
                c.TwoLetterISOLanguageName.Length == 2)             // Could return 3-letter names...
            .OrderBy(DetermineLanguageBias)                         // Push VIP(C) languages to the top of the drop-down
            .ThenBy(c => c.DisplayName)                   // If ranking is equivalent, order based on name
            .Select(c => new SelectOption
            {
                Name = c.DisplayName,
                Hint = c.TwoLetterISOLanguageName,
                Value = PackLanguage(c.TwoLetterISOLanguageName)    // Pack the language code into a 32-bit integer
            });

        languageOptions.AddRange(filteredLanguageOptions);
        return languageOptions;
    }

    private static int PackLanguage(ReadOnlySpan<char> language)
    {
        return (language[0] << 8) | language[1];
    }

    private static int DetermineLanguageBias(CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName switch
        {
            "en" => 0,
            _ => int.MaxValue
        };
    }
}
