using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Organizer
{
    public class FileNameBuilderTokenEqualityComparer : IEqualityComparer<string>
    {
        public static readonly FileNameBuilderTokenEqualityComparer Instance = new FileNameBuilderTokenEqualityComparer();

        private static readonly Regex SimpleTokenRegex = new Regex(@"\s|_|\W", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private FileNameBuilderTokenEqualityComparer()
        {
        }

        public bool Equals(string s1, string s2)
        {
            return SimplifyToken(s1).Equals(SimplifyToken(s2));
        }

        public int GetHashCode(string str)
        {
            return SimplifyToken(str).GetHashCode();
        }

        private static string SimplifyToken(string token)
        {
            return SimpleTokenRegex.Replace(token, string.Empty).ToLower();
        }
    }
}
