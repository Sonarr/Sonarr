using System;
using System.IO;

namespace NzbDrone.Core.Providers
{
    public class DiskProvider : IDiskProvider
    {
        #region IDiskProvider Members

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public String CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path).FullName;
        }

        #endregion

        public static string CleanPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path can not be null or empty");
            return path.ToLower().Trim('/', '\\', ' ');
        }
    }
}