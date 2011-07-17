using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core
{
    public static class Fluent
    {
        public static string WithDefault(this string actual, object defaultValue)
        {
            if (defaultValue == null)
                throw new ArgumentNullException("defaultValue");
            if (String.IsNullOrWhiteSpace(actual))
            {
                return defaultValue.ToString();
            }

            return actual;
        }


        public static double Megabytes(this int megabytes)
        {
            return megabytes * 1048576;
        }
    }
}
