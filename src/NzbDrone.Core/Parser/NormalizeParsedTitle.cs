using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Parser
{
    public static class NormalizeParsedTitle
    {
        private static readonly Regex NormalizeAtRegex = new Regex(@"(?<=^|\s)@ ?",
                                                                RegexOptions.Compiled);

        private static readonly Regex NormalizeAtARegex = new Regex(@"@",
                                                                RegexOptions.Compiled);

        private static readonly Regex NormalizePercentRegex = new Regex(@"(?<=(?:^|\s)\d+)%",
                                                                RegexOptions.Compiled);

        private static readonly Regex NormalizeRegex = new Regex(@"((?:\b|_)(?<!^)(a(?!$)|an|the|and|or|of)(?:\b|_))|\W|_",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex WordDelimiterRegex = new Regex(@"(\s|\.|,|_|-|=|\|)+", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);
        private static readonly Regex CommonWordRegex = new Regex(@"\b(a|an|the|and|or|of)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DuplicateSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);
        private static readonly Regex SpecialEpisodeWordRegex = new Regex(@"\b(part|special|edition|christmas)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string CleanSeriesTitle(this string title)
        {
            long number = 0;

            //If Title only contains numbers return it as is.
            if (long.TryParse(title, out number))
                return title;

            title = NormalizeAtRegex.Replace(title, "At ");

            title = NormalizeAtARegex.Replace(title, "a");

            title = NormalizePercentRegex.Replace(title, " percent");

            title = NormalizeRegex.Replace(title, string.Empty);

            return title.ToLower().RemoveAccent();
        }

        public static string NormalizeEpisodeTitle(string title)
        {
            title = SpecialEpisodeWordRegex.Replace(title, string.Empty);
            title = PunctuationRegex.Replace(title, " ");
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim()
                        .ToLower();
        }

        public static string NormalizeTitle(string title)
        {
            title = WordDelimiterRegex.Replace(title, " ");
            title = PunctuationRegex.Replace(title, string.Empty);
            title = CommonWordRegex.Replace(title, string.Empty);
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim().ToLower();
        }
    }
}
