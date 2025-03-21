using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Disk
{
    public struct OsPath : IEquatable<OsPath>
    {
        private readonly string _path;
        private readonly OsPathKind _kind;

        private static readonly Regex UncPathRegex = new Regex(@"(?<unc>^\\\\(?:\?\\UNC\\)?[^\\]+\\[^\\]+)(?:\\|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public OsPath(string path)
        {
            if (path == null)
            {
                _kind = OsPathKind.Unknown;
                _path = string.Empty;
            }
            else
            {
                _kind = DetectPathKind(path);
                _path = FixSlashes(path, _kind);
            }
        }

        public OsPath(string path, OsPathKind kind)
        {
            if (path == null)
            {
                _kind = kind;
                _path = string.Empty;
            }
            else
            {
                _kind = kind;
                _path = FixSlashes(path, kind);
            }
        }

        private static OsPathKind DetectPathKind(string path)
        {
            if (path.StartsWith("/"))
            {
                return OsPathKind.Unix;
            }

            if (HasWindowsDriveLetter(path) || path.Contains('\\'))
            {
                return OsPathKind.Windows;
            }

            if (path.Contains('/'))
            {
                return OsPathKind.Unix;
            }

            return OsPathKind.Unknown;
        }

        private static bool HasWindowsDriveLetter(string path)
        {
            if (path.Length < 2)
            {
                return false;
            }

            if (!char.IsLetter(path[0]) || path[1] != ':')
            {
                return false;
            }

            if (path.Length > 2 && path[2] != '\\' && path[2] != '/')
            {
                return false;
            }

            return true;
        }

        private static string FixSlashes(string path, OsPathKind kind)
        {
            switch (kind)
            {
                case OsPathKind.Windows:
                    return path.Replace('/', '\\');
                case OsPathKind.Unix:
                    path = path.Replace('\\', '/');
                    while (path.Contains("//"))
                    {
                        path = path.Replace("//", "/");
                    }

                    return path;
            }

            return path;
        }

        private static string TrimTrailingSlash(string path, OsPathKind kind)
        {
            switch (kind)
            {
                case OsPathKind.Windows when !path.EndsWith(":\\"):
                    while (!path.EndsWith(":\\") && path.EndsWith('\\'))
                    {
                        path = path[..^1];
                    }

                    return path;
                case OsPathKind.Unix when path != "/":
                    while (path != "/" && path.EndsWith('/'))
                    {
                        path = path[..^1];
                    }

                    return path;
            }

            return path;
        }

        public OsPathKind Kind => _kind;

        public bool IsWindowsPath => _kind == OsPathKind.Windows;

        public bool IsUnixPath => _kind == OsPathKind.Unix;

        public bool IsEmpty => _path.IsNullOrWhiteSpace();

        public bool IsRooted
        {
            get
            {
                if (IsWindowsPath)
                {
                    return _path.StartsWith(@"\\") || HasWindowsDriveLetter(_path);
                }

                if (IsUnixPath)
                {
                    return _path.StartsWith("/");
                }

                return false;
            }
        }

        public OsPath Directory
        {
            get
            {
                var index = GetFileNameIndex();

                if (index == -1)
                {
                    return Null;
                }

                var rootLength = GetRootLength();

                if (rootLength == _path.Length)
                {
                    return Null;
                }

                if (rootLength > index + 1)
                {
                    return new OsPath(_path.Substring(0, rootLength));
                }

                return new OsPath(_path.Substring(0, index), _kind).AsDirectory();
            }
        }

        public string FullPath => _path;

        public string PathWithoutTrailingSlash => TrimTrailingSlash(_path, _kind);

        public string FileName
        {
            get
            {
                var index = GetFileNameIndex();

                if (index == -1)
                {
                    var path = _path.Trim('\\', '/');

                    if (path.Length == 0)
                    {
                        return null;
                    }

                    return path;
                }

                return _path.Substring(index).Trim('\\', '/');
            }
        }

        public string Name
        {
            // Meant to behave similar to DirectoryInfo.Name

            get
            {
                var index = GetFileNameIndex();

                if (index == -1)
                {
                    return PathWithoutTrailingSlash;
                }

                var rootLength = GetRootLength();

                if (rootLength > index + 1)
                {
                    return _path.Substring(0, rootLength);
                }

                return TrimTrailingSlash(_path.Substring(index).TrimStart('/', '\\'), _kind);
            }
        }

        public bool IsValid => _path.IsPathValid(PathValidationType.CurrentOs);

        private int GetFileNameIndex()
        {
            if (_path.Length < 2)
            {
                return -1;
            }

            var index = _path.LastIndexOfAny(new[] { '/', '\\' }, _path.Length - 2);

            if (index == -1)
            {
                return -1;
            }

            if (_path.StartsWith(@"\\") && index < 2)
            {
                return -1;
            }

            if (_path.StartsWith("/") && index == 0)
            {
                index++;
            }

            return index;
        }

        private int GetRootLength()
        {
            if (!IsRooted)
            {
                return 0;
            }

            if (_kind == OsPathKind.Unix)
            {
                return 1;
            }

            if (_kind == OsPathKind.Windows)
            {
                if (HasWindowsDriveLetter(_path))
                {
                    return 3;
                }

                var uncMatch = UncPathRegex.Match(_path);

                // \\?\UNC\server\share\ or \\server\share
                if (uncMatch.Success)
                {
                    return uncMatch.Groups["unc"].Length;
                }

                // \\?\C:\
                if (_path.StartsWith(@"\\?\"))
                {
                    return 7;
                }
            }

            return 0;
        }

        private string[] GetFragments()
        {
            return _path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static OsPath Null => new(null);

        public override string ToString()
        {
            return _path;
        }

        public override int GetHashCode()
        {
            return _path.ToLowerInvariant().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is OsPath)
            {
                return Equals((OsPath)obj);
            }

            if (obj is string)
            {
                return Equals(new OsPath(obj as string));
            }

            return false;
        }

        public OsPath AsDirectory()
        {
            if (IsEmpty)
            {
                return this;
            }

            switch (Kind)
            {
                case OsPathKind.Windows:
                    return new OsPath(_path.TrimEnd('\\') + "\\", _kind);
                case OsPathKind.Unix:
                    return new OsPath(_path.TrimEnd('/') + "/", _kind);
            }

            return this;
        }

        public bool Contains(OsPath other)
        {
            if (!IsRooted || !other.IsRooted)
            {
                return false;
            }

            var leftFragments = GetFragments();
            var rightFragments = other.GetFragments();

            if (rightFragments.Length < leftFragments.Length)
            {
                return false;
            }

            var stringComparison = (Kind == OsPathKind.Windows || other.Kind == OsPathKind.Windows) ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

            for (var i = 0; i < leftFragments.Length; i++)
            {
                if (!string.Equals(leftFragments[i], rightFragments[i], stringComparison))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Equals(OsPath other)
        {
            return Equals(other, false);
        }

        public bool Equals(OsPath other, bool ignoreTrailingSlash)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (_path == other._path)
            {
                return true;
            }

            var left = ignoreTrailingSlash ? PathWithoutTrailingSlash : _path;
            var right = ignoreTrailingSlash ? other.PathWithoutTrailingSlash : other._path;

            if (Kind == OsPathKind.Windows || other.Kind == OsPathKind.Windows)
            {
                return string.Equals(left, right, StringComparison.InvariantCultureIgnoreCase);
            }

            return string.Equals(left, right, StringComparison.InvariantCulture);
        }

        public static bool operator ==(OsPath left, OsPath right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(OsPath left, OsPath right)
        {
            if (ReferenceEquals(left, null))
            {
                return !ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }

        public static OsPath operator +(OsPath left, OsPath right)
        {
            if (left.Kind != right.Kind && right.Kind != OsPathKind.Unknown)
            {
                throw new Exception(string.Format("Cannot combine OsPaths of different platforms ('{0}' + '{1}')", left, right));
            }

            if (right.IsEmpty)
            {
                return left;
            }

            if (right.IsRooted)
            {
                return right;
            }

            if (left.Kind == OsPathKind.Windows || right.Kind == OsPathKind.Windows)
            {
                return new OsPath(string.Join("\\", left._path.TrimEnd('\\'), right._path.TrimStart('\\')), OsPathKind.Windows);
            }

            if (left.Kind == OsPathKind.Unix || right.Kind == OsPathKind.Unix)
            {
                return new OsPath(string.Join("/", left._path.TrimEnd('/'), right._path), OsPathKind.Unix);
            }

            return new OsPath(string.Join("/", left._path, right._path), OsPathKind.Unknown);
        }

        public static OsPath operator +(OsPath left, string right)
        {
            return left + new OsPath(right);
        }

        public static OsPath operator -(OsPath left, OsPath right)
        {
            if (!left.IsRooted || !right.IsRooted)
            {
                throw new ArgumentException("Cannot determine relative path for unrooted paths.");
            }

            var leftFragments = left.GetFragments();
            var rightFragments = right.GetFragments();

            var stringComparison = (left.Kind == OsPathKind.Windows || right.Kind == OsPathKind.Windows) ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

            int i;
            for (i = 0; i < leftFragments.Length && i < rightFragments.Length; i++)
            {
                if (!string.Equals(leftFragments[i], rightFragments[i], stringComparison))
                {
                    break;
                }
            }

            if (i == 0)
            {
                return right;
            }

            var newFragments = new List<string>();

            for (var j = i; j < rightFragments.Length; j++)
            {
                newFragments.Add("..");
            }

            for (var j = i; j < leftFragments.Length; j++)
            {
                newFragments.Add(leftFragments[j]);
            }

            if (left.FullPath.EndsWith("\\") || left.FullPath.EndsWith("/"))
            {
                newFragments.Add(string.Empty);
            }

            if (left.Kind == OsPathKind.Windows || right.Kind == OsPathKind.Windows)
            {
                return new OsPath(string.Join("\\", newFragments), OsPathKind.Unknown);
            }

            return new OsPath(string.Join("/", newFragments), OsPathKind.Unknown);
        }
    }

    public enum OsPathKind
    {
        Unknown,
        Windows,
        Unix
    }
}
