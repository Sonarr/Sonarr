using System;
using System.IO;

namespace NzbDrone.Core.Providers
{
    public class DiskProvider : IDiskProvider
    {
        #region IDiskProvider Members

        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public string[] GetFiles(string path, string pattern, SearchOption searchOption)
        {
            return Directory.GetFiles(path, pattern, searchOption);
        }

        public long GetSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public String CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path).FullName;
        }

        #endregion
    }
}