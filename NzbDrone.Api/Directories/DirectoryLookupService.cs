using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common;

namespace NzbDrone.Api.Directories
{
    public interface IDirectoryLookupService
    {
        List<string> LookupSubDirectories(string query);
    }

    public class DirectoryLookupService : IDirectoryLookupService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly HashSet<string> _setToRemove = new HashSet<string> { "$Recycle.Bin", "System Volume Information" };

        public DirectoryLookupService(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public List<string> LookupSubDirectories(string query)
        {
            var dirs = new List<string>();
            var lastSeparatorIndex = query.LastIndexOf(Path.DirectorySeparatorChar);
            var path = query.Substring(0, lastSeparatorIndex + 1);

            if (lastSeparatorIndex != -1)
            {
                dirs = GetSubDirectories(path);
                dirs.RemoveAll(x => _setToRemove.Contains(new DirectoryInfo(x).Name));
            }

            return dirs;
        }


        private List<string> GetSubDirectories(string path)
        {
            try
            {
                return _diskProvider.GetDirectories(path).ToList();
            }
            catch (DirectoryNotFoundException)
            {
                return new List<string>();
                
            }
            catch (ArgumentException)
            {
                return new List<string>();
            }
        }
    }
}
