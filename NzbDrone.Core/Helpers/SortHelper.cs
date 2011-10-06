using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Helpers
{
    public class SortHelper
    {
        public static string SkipArticles(string input)
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
