using System;
using Mono.Unix;
using Mono.Unix.Native;
using NLog;

namespace NzbDrone.Mono.Disk
{
    public interface ISymbolicLinkResolver
    {
        string GetCompleteRealPath(string path);
    }

    public class SymbolicLinkResolver : ISymbolicLinkResolver
    {
        private readonly Logger _logger;

        public SymbolicLinkResolver(Logger logger)
        {
            _logger = logger;
        }

        public string GetCompleteRealPath(string path)
        {
            if (path == null)
            {
                return null;
            }

            try
            {
                var realPath = path;
                for (var links = 0; links < 32; links++)
                {
                    var wasSymLink = TryFollowFirstSymbolicLink(ref realPath);
                    if (!wasSymLink)
                    {
                        return realPath;
                    }
                }

                var ex = new UnixIOException(Errno.ELOOP);
                _logger.Warn("Failed to check for symlinks in the path {0}: {1}", path, ex.Message);
                return path;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to check for symlinks in the path {0}", path);
                return path;
            }
        }

        private static void GetPathComponents(string path, out string[] components, out int lastIndex)
        {
            var dirs = path.Split(UnixPath.DirectorySeparatorChar);
            var target = 0;
            for (var i = 0; i < dirs.Length; ++i)
            {
                if (dirs[i] == "." || string.IsNullOrEmpty(dirs[i]))
                {
                    continue;
                }

                if (dirs[i] == "..")
                {
                    if (target != 0)
                    {
                        target--;
                    }
                    else
                    {
                        target++;
                    }
                }
                else
                {
                    dirs[target++] = dirs[i];
                }
            }

            components = dirs;
            lastIndex = target;
        }

        private bool TryFollowFirstSymbolicLink(ref string path)
        {
            string[] dirs;
            int lastIndex;
            GetPathComponents(path, out dirs, out lastIndex);

            if (lastIndex == 0)
            {
                return false;
            }

            var realPath = "";

            for (var i = 0; i < lastIndex; ++i)
            {
                if (i != 0 || UnixPath.IsPathRooted(path))
                {
                    realPath = string.Concat(realPath, UnixPath.DirectorySeparatorChar, dirs[i]);
                }
                else
                {
                    realPath = string.Concat(realPath, dirs[i]);
                }

                var pathValid = TryFollowSymbolicLink(ref realPath, out var wasSymLink);

                if (!pathValid || wasSymLink)
                {
                    // If the path does not exist, or it was a symlink then we need to concat the remaining dir components and start over (or return)
                    var count = lastIndex - i - 1;
                    if (count > 0)
                    {
                        realPath = string.Concat(realPath, UnixPath.DirectorySeparatorChar, string.Join(UnixPath.DirectorySeparatorChar.ToString(), dirs, i + 1, lastIndex - i - 1));
                    }

                    path = realPath;
                    return pathValid;
                }
            }

            return false;
        }

        private bool TryFollowSymbolicLink(ref string path, out bool wasSymLink)
        {
            if (!UnixFileSystemInfo.TryGetFileSystemEntry(path, out var fsentry) || !fsentry.Exists)
            {
                wasSymLink = false;
                return false;
            }

            if (!fsentry.IsSymbolicLink)
            {
                wasSymLink = false;
                return true;
            }

            var link = UnixPath.TryReadLink(path);

            if (link == null)
            {
                var errno = Stdlib.GetLastError();
                if (errno != Errno.EINVAL)
                {
                    _logger.Trace("Checking path {0} for symlink returned error {1}, assuming it's not a symlink.", path, errno);
                }

                wasSymLink = true;
                return false;
            }
            else
            {
                if (UnixPath.IsPathRooted(link))
                {
                    path = link;
                }
                else
                {
                    path = UnixPath.GetDirectoryName(path) + UnixPath.DirectorySeparatorChar + link;
                    path = UnixPath.GetCanonicalPath(path);
                }

                wasSymLink = true;
                return true;
            }
        }
    }
}
