using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common
{
    public class PathEqualityComparer : IEqualityComparer<String>
    {
        public bool Equals(string x, string y)
        {
            return x.PathEquals(y);
        }

        public int GetHashCode(string obj)
        {
            return obj.CleanFilePath().GetHashCode();
        }
    }
}
