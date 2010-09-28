using System;
using System.IO;

namespace NzbDrone.Core.Controllers
{
    public class DiskController : IDiskController
    {
        #region IDiskController Members

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
    }
}