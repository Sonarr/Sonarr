using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Organizer
{
    public class FilenameBuilderTokenEqualityComparer : IEqualityComparer<String>
    {
        public static readonly FilenameBuilderTokenEqualityComparer Instance = new FilenameBuilderTokenEqualityComparer();

        private static readonly Regex SimpleTokenRegex = new Regex(@"\s|_|\W", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private FilenameBuilderTokenEqualityComparer()
        {
            
        }

        public bool Equals(String s1, String s2)
        {
            return SimplifyToken(s1).Equals(SimplifyToken(s2));
        }

        public int GetHashCode(String str)
        {
            return SimplifyToken(str).GetHashCode();
        }

        private static string SimplifyToken(string token)
        {
            return SimpleTokenRegex.Replace(token, String.Empty).ToLower();
        }
    }
}
