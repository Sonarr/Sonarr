using System;
using System.Collections.Generic;
using NzbDrone.Common.EnvironmentInfo;

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
            if (OsInfo.IsLinux)
            {
                return obj.CleanFilePath().GetHashCode();
            }

            return obj.CleanFilePath().ToLower().GetHashCode();
        }
    }
}
