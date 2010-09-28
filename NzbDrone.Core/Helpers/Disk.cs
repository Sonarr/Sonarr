using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Helpers
{

    static class Disk
    {

        /// <summary>
        /// Cleans the path. making it a uniform path.
        /// this will normalize all different presentations of a single folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Cleaned Path</returns>
        public static string CleanPath(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path can not be null or empty");
            return path.ToLower().Trim('/', '\\', ' ');
        }

    }
}
