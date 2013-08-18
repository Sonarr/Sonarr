using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NzbDrone.Common
{
    public static class StringExtensions
    {
        public static string NullSafe(this string target)
        {
            return ((object)target).NullSafe().ToString();
        }

        public static object NullSafe(this object target)
        {
            if (target != null) return target;
            return "[NULL]";
        }

        public static string FirstCharToUpper(this string input)
        {
            return input.First().ToString().ToUpper() + String.Join("", input.Skip(1));
        }

        public static string Inject(this string format, params object[] formattingArgs)
        {
            return string.Format(format, formattingArgs);
        }

        private static readonly Regex CollapseSpace = new Regex(@"\s+", RegexOptions.Compiled);

        public static string RemoveAccent(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string CleanSpaces(this string text)
        {
            return CollapseSpace.Replace(text, " ").Trim();
        }
    }
}