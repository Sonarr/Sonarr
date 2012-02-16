using System;
using System.Diagnostics;

namespace NzbDrone.Common
{
    public static class StringExtention
    {

        public static object NullCheck(this object target)
        {
            if (target != null) return target;
            return "[NULL]";
        }

        public static string NullCheck(this string target)
        {
            return ((object)target).NullCheck().ToString();
        }
    }
}