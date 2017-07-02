using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Organizer
{
    public static class NormalizeOfficialTitle
    {
        private static readonly Regex ScenifyAtToAt = new Regex(@"^@\s?|\s@\s?|(?<=[a-z])@(?=[A-Z])", RegexOptions.Compiled);
        private static readonly Regex ScenifyAtToA = new Regex(@"@(?=[A-Z])|(?<=[A-Z]{2})@", RegexOptions.Compiled);
        private static readonly Regex ScenifyAtToa = new Regex(@"@", RegexOptions.Compiled);

        private static readonly Regex ScenifyPercentDigit = new Regex(@"(?<=(?:^|\s)\d+)%", RegexOptions.Compiled);
        private static readonly Regex ScenifyPercent = new Regex(@"%", RegexOptions.Compiled);

        private static readonly Regex ScenifyAnd = new Regex(@"\s?&\s?", RegexOptions.Compiled);

        private static readonly Regex ScenifyColonToSpace = new Regex(@"\s?:\s?", RegexOptions.Compiled);

        // Needs to go before Dollar handling.
        private static readonly Regex ScenifyCommaDigit = new Regex(@"(?<=\d),(?=\d{3}(,\d{3})*)", RegexOptions.Compiled);
        private static readonly Regex ScenifyCommaToSpace = new Regex(@"\s?,\s?", RegexOptions.Compiled);

        private static readonly Regex ScenifyDollarDigit = new Regex(@"\$\s?([0-9.,]+)(?=\s|$)", RegexOptions.Compiled);
        private static readonly Regex ScenifyDollarToDollar = new Regex(@"(?<=^|\s)\$+(?=\s|$)", RegexOptions.Compiled);
        private static readonly Regex ScenifyDollarToS = new Regex(@"(?<=^|\s)\$", RegexOptions.Compiled);
        private static readonly Regex ScenifyDollarTos = new Regex(@"\$", RegexOptions.Compiled);

        private static readonly Regex ScenifyQuoteToSpace = new Regex(@"(?<=[a-zA-Z])'(?=[A-Z])", RegexOptions.Compiled);
        private static readonly Regex ScenifyQuote = new Regex(@"'", RegexOptions.Compiled);

        private static readonly Regex ScenifySemiColonToSpace = new Regex(@"\s*;\s*", RegexOptions.Compiled);

        private static readonly Regex ScenifyRemoveChars = new Regex(@"[?!""]", RegexOptions.Compiled);

        private static readonly Regex ScenifyRemoveUnknownChars = new Regex(@"[<>]", RegexOptions.Compiled);

        // These Regexes do not have appropriate testcases and should be treated with caution when modified.
        private static readonly Regex ScenifyRemoveCharsOld = new Regex(@"(?<=\s)[|`~^*=_-](?=\s)|[(){}[\]]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ScenifyReplaceCharsOld = new Regex(@"[\\/]", RegexOptions.Compiled);

        public static string ScenifyTitle(string title)
        {
            title = ScenifyAtToAt.Replace(title, m => m.Index == 0 ? "At " : " at ");
            title = ScenifyAtToA.Replace(title, "A");
            title = ScenifyAtToa.Replace(title, "a");

            title = ScenifyPercentDigit.Replace(title, " Percent");
            title = ScenifyPercent.Replace(title, "");

            title = ScenifyAnd.Replace(title, " and ");

            title = ScenifyColonToSpace.Replace(title, " ");

            title = ScenifyCommaDigit.Replace(title, "");
            title = ScenifyCommaToSpace.Replace(title, " ");

            title = ScenifyDollarDigit.Replace(title, "$1"); // "$1 Dollar"
            title = ScenifyDollarToDollar.Replace(title, "Dollar");
            title = ScenifyDollarToS.Replace(title, "S");
            title = ScenifyDollarTos.Replace(title, "s");

            title = ScenifyQuoteToSpace.Replace(title, " ");
            title = ScenifyQuote.Replace(title, "");

            title = ScenifySemiColonToSpace.Replace(title, " ");

            title = ScenifyRemoveChars.Replace(title, "");

            title = ScenifyRemoveUnknownChars.Replace(title, "");

            title = ScenifyReplaceCharsOld.Replace(title, " ");
            title = ScenifyRemoveCharsOld.Replace(title, string.Empty);

            return title;
        }

    }
}
