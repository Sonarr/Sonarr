using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Test.Languages
{
    [TestFixture]
    public class LanguageFixture : CoreTest
    {
        public static object[] FromIntCases =
            {
                new object[] {1, Language.English},
                new object[] {2, Language.French},
                new object[] {3, Language.Spanish},
                new object[] {4, Language.German},
                new object[] {5, Language.Italian},
                new object[] {6, Language.Danish},
                new object[] {7, Language.Dutch},
                new object[] {8, Language.Japanese},
                new object[] {9, Language.Cantonese},
                new object[] {10, Language.Mandarin},
                new object[] {11, Language.Russian},
                new object[] {12, Language.Polish},
                new object[] {13, Language.Vietnamese},
                new object[] {14, Language.Swedish},
                new object[] {15, Language.Norwegian},
                new object[] {16, Language.Finnish},
                new object[] {17, Language.Turkish},
                new object[] {18, Language.Portuguese},
                new object[] {19, Language.Flemish},
                new object[] {20, Language.Greek},
                new object[] {21, Language.Korean},
                new object[] {22, Language.Hungarian}
            };

        public static object[] ToIntCases =
            {
                new object[] {Language.English, 1},
                new object[] {Language.French, 2},
                new object[] {Language.Spanish, 3},
                new object[] {Language.German, 4},
                new object[] {Language.Italian, 5},
                new object[] {Language.Danish, 6},
                new object[] {Language.Dutch, 7},
                new object[] {Language.Japanese, 8},
                new object[] {Language.Cantonese, 9},
                new object[] {Language.Mandarin, 10},
                new object[] {Language.Russian, 11},
                new object[] {Language.Polish, 12},
                new object[] {Language.Vietnamese, 13},
                new object[] {Language.Swedish, 14},
                new object[] {Language.Norwegian, 15},
                new object[] {Language.Finnish, 16},
                new object[] {Language.Turkish, 17},
                new object[] {Language.Portuguese, 18},
                new object[] {Language.Flemish, 19},
                new object[] {Language.Greek, 20},
                new object[] {Language.Korean, 21},
                new object[] {Language.Hungarian, 22}
            };

        [Test, TestCaseSource("FromIntCases")]
        public void should_be_able_to_convert_int_to_languageTypes(int source, Language expected)
        {
            var language = (Language)source;
            language.Should().Be(expected);
        }

        [Test, TestCaseSource("ToIntCases")]
        public void should_be_able_to_convert_languageTypes_to_int(Language source, int expected)
        {
            var i = (int)source;
            i.Should().Be(expected);
        }

        public static List<ProfileLanguageItem> GetDefaultLanguages(params Language[] allowed)
        {
            var languages = new List<Language>
            {
                Language.English,
                Language.Spanish,
                Language.French
            };

            if (allowed.Length == 0)
                allowed = languages.ToArray();

            var items = languages
                .Except(allowed)
                .Concat(allowed)
                .Select(v => new ProfileLanguageItem { Language = v, Allowed = allowed.Contains(v) }).ToList();

            return items;
        }
    }
}
