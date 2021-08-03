using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NzbDrone.Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex CamelCaseRegex = new Regex("(?<!^)[A-Z]", RegexOptions.Compiled);

        public static string NullSafe(this string target)
        {
            return ((object)target).NullSafe().ToString();
        }

        public static object NullSafe(this object target)
        {
            if (target != null)
            {
                return target;
            }

            return "[NULL]";
        }

        public static string FirstCharToLower(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return char.ToLowerInvariant(input.First()) + input.Substring(1);
        }

        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return char.ToUpperInvariant(input.First()) + input.Substring(1);
        }

        public static string Inject(this string format, params object[] formattingArgs)
        {
            return string.Format(format, formattingArgs);
        }

        private static readonly Regex CollapseSpace = new Regex(@"\s+", RegexOptions.Compiled);

        public static string Replace(this string text, int index, int length, string replacement)
        {
            text = text.Remove(index, length);
            text = text.Insert(index, replacement);
            return text;
        }

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

        public static string TrimEnd(this string text, string postfix)
        {
            if (text.EndsWith(postfix))
            {
                text = text.Substring(0, text.Length - postfix.Length);
            }

            return text;
        }

        public static string Join(this IEnumerable<string> values, string separator)
        {
            return string.Join(separator, values);
        }

        public static string CleanSpaces(this string text)
        {
            return CollapseSpace.Replace(text, " ").Trim();
        }

        public static bool IsNullOrWhiteSpace(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        public static bool IsNotNullOrWhiteSpace(this string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }

        public static bool StartsWithIgnoreCase(this string text, string startsWith)
        {
            return text.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EndsWithIgnoreCase(this string text, string startsWith)
        {
            return text.EndsWith(startsWith, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EqualsIgnoreCase(this string text, string equals)
        {
            return text.Equals(equals, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool ContainsIgnoreCase(this string text, string contains)
        {
            return text.IndexOf(contains, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public static string WrapInQuotes(this string text)
        {
            if (!text.Contains(" "))
            {
                return text;
            }

            return "\"" + text + "\"";
        }

        public static byte[] HexToByteArray(this string input)
        {
            return Enumerable.Range(0, input.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(input.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string ToHexString(this byte[] input)
        {
            return string.Concat(Array.ConvertAll(input, x => x.ToString("X2")));
        }

        public static string FromOctalString(this string octalValue)
        {
            octalValue = octalValue.TrimStart('\\');

            var first = int.Parse(octalValue.Substring(0, 1));
            var second = int.Parse(octalValue.Substring(1, 1));
            var third = int.Parse(octalValue.Substring(2, 1));
            var byteResult = (byte)((first << 6) | (second << 3) | third);

            return Encoding.ASCII.GetString(new[] { byteResult });
        }

        public static string SplitCamelCase(this string input)
        {
            return CamelCaseRegex.Replace(input, match => " " + match.Value);
        }

        public static bool ContainsIgnoreCase(this IEnumerable<string> source, string value)
        {
            return source.Contains(value, StringComparer.InvariantCultureIgnoreCase);
        }

        public static string ToUrlSlug(this string value)
        {
            //First to lower case
            value = value.ToLowerInvariant();

            //Remove all accents
            value = value.RemoveAccent();

            //Replace spaces
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);

            //Remove invalid chars
            value = Regex.Replace(value, @"[^a-z0-9\s-_]", "", RegexOptions.Compiled);

            //Trim dashes from end
            value = value.Trim('-', '_');

            //Replace double occurences of - or _
            value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return value;
        }
    }
}
