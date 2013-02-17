
using System.IO;

namespace NzbDrone.Common
{
    public static class StringExtention
    {

        public static object NullSafe(this object target)
        {
            if (target != null) return target;
            return "[NULL]";
        }

        public static string NullSafe(this string target)
        {
            return ((object)target).NullSafe().ToString();
        }
    }
}