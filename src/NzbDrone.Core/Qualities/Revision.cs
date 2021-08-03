using System;
using System.Text;

namespace NzbDrone.Core.Qualities
{
    public class Revision : IEquatable<Revision>, IComparable<Revision>
    {
        private Revision()
        {
        }

        public Revision(int version = 1, int real = 0, bool isRepack = false)
        {
            Version = version;
            Real = real;
            IsRepack = isRepack;
        }

        public int Version { get; set; }
        public int Real { get; set; }
        public bool IsRepack { get; set; }

        public bool Equals(Revision other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return other.Version.Equals(Version) && other.Real.Equals(Real);
        }

        public int CompareTo(Revision other)
        {
            if (Real > other.Real)
            {
                return 1;
            }

            if (Real < other.Real)
            {
                return -1;
            }

            if (Version > other.Version)
            {
                return 1;
            }

            if (Version < other.Version)
            {
                return -1;
            }

            return 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("v{0}", Version);

            if (Real > 0)
            {
                sb.AppendFormat(" Real:{0}", Real);
            }

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return Version ^ Real << 8;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as Revision);
        }

        public static bool operator ==(Revision left, Revision right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Revision left, Revision right)
        {
            return !Equals(left, right);
        }

        public static bool operator >(Revision left, Revision right)
        {
            if (ReferenceEquals(null, left))
            {
                return false;
            }

            if (ReferenceEquals(null, right))
            {
                return true;
            }

            return left.CompareTo(right) > 0;
        }

        public static bool operator <(Revision left, Revision right)
        {
            if (ReferenceEquals(null, left))
            {
                return true;
            }

            if (ReferenceEquals(null, right))
            {
                return false;
            }

            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(Revision left, Revision right)
        {
            if (ReferenceEquals(null, left))
            {
                return false;
            }

            if (ReferenceEquals(null, right))
            {
                return true;
            }

            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(Revision left, Revision right)
        {
            if (ReferenceEquals(null, left))
            {
                return true;
            }

            if (ReferenceEquals(null, right))
            {
                return false;
            }

            return left.CompareTo(right) <= 0;
        }
    }
}
