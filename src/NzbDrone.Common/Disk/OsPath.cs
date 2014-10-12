using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Common.Disk
{
    public struct OsPath : IEquatable<OsPath>
    {
        private readonly String _path;
        private readonly OsPathKind _kind;

        public OsPath(String path)
        {
            if (path == null)
            {
                _kind = OsPathKind.Unknown;
                _path = String.Empty;
            }
            else
            {
                _kind = DetectPathKind(path);
                _path = FixSlashes(path, _kind);
            }
        }

        public OsPath(String path, OsPathKind kind)
        {
            if (path == null)
            {
                _kind = kind;
                _path = String.Empty;
            }
            else
            {
                _kind = kind;
                _path = FixSlashes(path, kind);
            }
        }

        private static OsPathKind DetectPathKind(String path)
        {
            if (path.StartsWith("/"))
            {
                return OsPathKind.Unix;
            }
            if (path.Contains(':') || path.Contains('\\'))
            {
                return OsPathKind.Windows;
            }
            else if (path.Contains('/'))
            {
                return OsPathKind.Unix;
            }
            else
            {
                return OsPathKind.Unknown;
            }
        }

        private static String FixSlashes(String path, OsPathKind kind)
        {
            if (kind == OsPathKind.Windows)
            {
                return path.Replace('/', '\\');
            }
            else if (kind == OsPathKind.Unix)
            {
                return path.Replace('\\', '/');
            }

            return path;
        }

        public OsPathKind Kind
        {
            get { return _kind; }
        }

        public Boolean IsWindowsPath
        {
            get { return _kind == OsPathKind.Windows; }
        }

        public Boolean IsUnixPath
        {
            get { return _kind == OsPathKind.Unix; }
        }

        public Boolean IsEmpty
        {
            get
            {
                return _path.IsNullOrWhiteSpace();
            }
        }

        public Boolean IsRooted
        {
            get
            {
                if (IsWindowsPath)
                {
                    return _path.StartsWith(@"\\") || _path.Contains(':');
                }
                else if (IsUnixPath)
                {
                    return _path.StartsWith("/");
                }
                else
                {
                    return false;
                }
            }
        }

        public OsPath Directory
        {
            get
            {
                var index = GetFileNameIndex();

                if (index == -1)
                {
                    return new OsPath(null);
                }
                else
                {
                    return new OsPath(_path.Substring(0, index), _kind).AsDirectory();
                }
            }
        }

        public String FullPath
        {
            get
            {
                return _path;
            }
        }

        public String FileName
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
                else
                {
                    return _path.Substring(index).Trim('\\', '/');
                }
            }
        }

        private Int32 GetFileNameIndex()
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

        private String[] GetFragments()
        {
            return _path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public override String ToString()
        {
            return _path;
        }

        public override Int32 GetHashCode()
        {
            return _path.ToLowerInvariant().GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            if (obj is OsPath)
            {
                return Equals((OsPath)obj);
            }
            else if (obj is String)
            {
                return Equals(new OsPath(obj as String));
            }
            else
            {
                return false;
            }
        }

        public OsPath AsDirectory()
        {
            if (IsEmpty)
            {
                return this;
            }

            if (Kind == OsPathKind.Windows)
            {
                return new OsPath(_path.TrimEnd('\\') + "\\", _kind);
            }
            else if (Kind == OsPathKind.Unix)
            {
                return new OsPath(_path.TrimEnd('/') + "/", _kind);
            }
            else
            {
                return this;
            }
        }

        public Boolean Contains(OsPath other)
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

            for (int i = 0; i < leftFragments.Length; i++)
            {
                if (!String.Equals(leftFragments[i], rightFragments[i], stringComparison))
                {
                    return false;
                }
            }

            return true;
        }

        public Boolean Equals(OsPath other)
        {
            if (ReferenceEquals(other, null)) return false;

            if (_path == other._path)
            {
                return true;
            }

            var left = _path;
            var right = other._path;

            if (Kind == OsPathKind.Windows || other.Kind == OsPathKind.Windows)
            {
                return String.Equals(left, right, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                return String.Equals(left, right, StringComparison.InvariantCulture);
            }
        }

        public static Boolean operator ==(OsPath left, OsPath right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);

            return left.Equals(right);
        }

        public static Boolean operator !=(OsPath left, OsPath right)
        {
            if (ReferenceEquals(left, null)) return !ReferenceEquals(right, null);

            return !left.Equals(right);
        }

        public static OsPath operator +(OsPath left, OsPath right)
        {
            if (left.Kind != right.Kind && right.Kind != OsPathKind.Unknown)
            {
                throw new Exception(String.Format("Cannot combine OsPaths of different platforms ('{0}' + '{1}')", left, right));
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
                return new OsPath(String.Join("\\", left._path.TrimEnd('\\'), right._path.TrimStart('\\')), OsPathKind.Windows);
            }
            else if (left.Kind == OsPathKind.Unix || right.Kind == OsPathKind.Unix)
            {
                return new OsPath(String.Join("/", left._path.TrimEnd('/'), right._path), OsPathKind.Unix);
            }
            else
            {
                return new OsPath(String.Join("/", left._path, right._path), OsPathKind.Unknown);
            }
        }

        public static OsPath operator +(OsPath left, String right)
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
                if (!String.Equals(leftFragments[i], rightFragments[i], stringComparison))
                {
                    break;
                }
            }

            if (i == 0)
            {
                return right;
            }

            var newFragments = new List<String>();

            for (int j = i; j < rightFragments.Length; j++)
            {
                newFragments.Add("..");
            }

            for (int j = i; j < leftFragments.Length; j++)
            {
                newFragments.Add(leftFragments[j]);
            }

            if (left.FullPath.EndsWith("\\") || left.FullPath.EndsWith("/"))
            {
                newFragments.Add(String.Empty);
            }

            if (left.Kind == OsPathKind.Windows || right.Kind == OsPathKind.Windows)
            {
                return new OsPath(String.Join("\\", newFragments), OsPathKind.Unknown);
            }
            else
            {
                return new OsPath(String.Join("/", newFragments), OsPathKind.Unknown);
            }
        }
    }

    public enum OsPathKind
    {
        Unknown,
        Windows,
        Unix
    }
}
