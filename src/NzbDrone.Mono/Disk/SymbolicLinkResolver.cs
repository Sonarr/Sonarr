using System;
using System.Text;
using Mono.Unix;
using Mono.Unix.Native;
using NLog;

namespace NzbDrone.Mono.Disk
{
    public interface ISymbolicLinkResolver
    {
        string GetCompleteRealPath(string path);
    }

    // Mono's own implementation doesn't handle exceptions very well.
    // All of this code was copied from mono with minor changes.
    public class SymbolicLinkResolver : ISymbolicLinkResolver
    {
        private readonly Logger _logger;

        public SymbolicLinkResolver(Logger logger)
        {
            _logger = logger;
        }

        public string GetCompleteRealPath(string path)
        {
            if (path == null) return null;

            try
            {
                string[] dirs;
                int lastIndex;
                GetPathComponents(path, out dirs, out lastIndex);

                var realPath = new StringBuilder();
                if (dirs.Length > 0)
                {
                    var dir = UnixPath.IsPathRooted(path) ? "/" : "";
                    dir += dirs[0];
                    realPath.Append(GetRealPath(dir));
                }
                for (var i = 1; i < lastIndex; ++i)
                {
                    realPath.Append("/").Append(dirs[i]);
                    var realSubPath = GetRealPath(realPath.ToString());
                    realPath.Remove(0, realPath.Length);
                    realPath.Append(realSubPath);
                }
                return realPath.ToString();
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, string.Format("Failed to check for symlinks in the path {0}", path));
                return path;
            }
        }


        private static void GetPathComponents(string path, out string[] components, out int lastIndex)
        {
            var dirs = path.Split(UnixPath.DirectorySeparatorChar);
            var target = 0;
            for (var i = 0; i < dirs.Length; ++i)
            {
                if (dirs[i] == "." || dirs[i] == string.Empty)
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

        public string GetRealPath(string path)
        {
            do
            {
                var link = UnixPath.TryReadLink(path);

                if (link == null)
                {
                    var errno = Stdlib.GetLastError();
                    if (errno != Errno.EINVAL)
                    {
                        _logger.Trace("Checking path {0} for symlink returned error {1}, assuming it's not a symlink.", path, errno);
                    }

                    return path;
                }

                if (UnixPath.IsPathRooted(link))
                {
                    path = link;
                }
                else
                {
                    path = UnixPath.GetDirectoryName(path) + UnixPath.DirectorySeparatorChar + link;
                    path = UnixPath.GetCanonicalPath(path);
                }
            } while (true);
        } 

    }
}
