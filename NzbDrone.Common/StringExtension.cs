using System;
using System.Linq;

namespace NzbDrone.Common
{
    public static class StringExtension
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
    }
}