using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Organizer
{
    public class FilenameBuilderTokenEqualityComparer : IEqualityComparer<String>
    {
        private static readonly Regex SimpleTokenRegex = new Regex(@"\s|_|\W", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool Equals(String s1, String s2)
        {
            return SimplifyToken(s1).Equals(SimplifyToken(s2));
        }

        public int GetHashCode(String str)
        {
            return SimplifyToken(str).GetHashCode();
        }

        private string SimplifyToken(string token)
        {
            return SimpleTokenRegex.Replace(token, String.Empty).ToLower();
        }
    }
}
