using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Helpers
{
    public static class SortHelper
    {
        public static string IgnoreArticles(this string input)
        {
            if (String.IsNullOrEmpty(input))
                return String.Empty;

            var articles = new List<string> { "The ", "An ", "A " };

            foreach (string article in articles)
            {
                if (input.ToLower().StartsWith(article, StringComparison.InvariantCultureIgnoreCase))
                    return input.Substring(article.Length).Trim();
            }

            return input;
        }
    }
}
