using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NzbDrone.Common
{
    public interface IDirectoryLookupService
    {
        List<String> LookupSubDirectories(string query);
    }

    public class DirectoryLookupService : IDirectoryLookupService
    {
        private readonly IDiskProvider _diskProvider;

        public DirectoryLookupService(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public List<String> LookupSubDirectories(string query)
        {
            List<String> dirs = null;
            try
            {
                //Windows (Including UNC)
                var windowsSep = query.LastIndexOf('\\');

                if (windowsSep > -1)
                {
                    var path = query.Substring(0, windowsSep + 1);
                    var dirsList = _diskProvider.GetDirectories(path).ToList();

                    if (Path.GetPathRoot(path).Equals(path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var setToRemove = new HashSet<string> { "$Recycle.Bin", "System Volume Information" };
                        dirsList.RemoveAll(x => setToRemove.Contains(new DirectoryInfo(x).Name));
                    }

                    dirs = dirsList;
                }

                //Unix
                var index = query.LastIndexOf('/');

                if (index > -1)
                {
                    dirs = _diskProvider.GetDirectories(query.Substring(0, index + 1)).ToList();
                }
            }
            catch (Exception)
            {
                //Swallow the exceptions so proper JSON is returned to the client (Empty results)
                return new List<string>();
            }

            return dirs;
        }
    }
}
