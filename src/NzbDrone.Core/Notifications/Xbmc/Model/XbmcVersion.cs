using System;

namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class XbmcVersion : IComparable<XbmcVersion>
    {
        public XbmcVersion()
        {
        }

        public XbmcVersion(int major)
        {
            Major = major;
        }

        public XbmcVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }

        public int CompareTo(XbmcVersion other)
        {
            if (other.Major > Major)
            {
                return -1;
            }

            if (other.Major < Major)
            {
                return 1;
            }

            if (other.Minor > Minor)
            {
                return -1;
            }

            if (other.Minor < Minor)
            {
                return 1;
            }

            if (other.Patch > Patch)
            {
                return -1;
            }

            if (other.Patch < Patch)
            {
                return 1;
            }

            return 0;
        }

        public static bool operator !=(XbmcVersion x, XbmcVersion y)
        {
            return !(x == y);
        }

        public static bool operator ==(XbmcVersion x, XbmcVersion y)
        {
            var xObj = (object)x;
            var yObj = (object)y;

            if (xObj == null || yObj == null)
            {
                return xObj == yObj;
            }

            return x.CompareTo(y) == 0;
        }

        public static bool operator >(XbmcVersion x, XbmcVersion y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <(XbmcVersion x, XbmcVersion y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator <=(XbmcVersion x, XbmcVersion y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >=(XbmcVersion x, XbmcVersion y)
        {
            return x.CompareTo(y) >= 0;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Overflow is fine, just wrap
                int hash = 17;
                hash = (hash * 23) + Major.GetHashCode();
                hash = (hash * 23) + Minor.GetHashCode();
                hash = (hash * 23) + Patch.GetHashCode();
                return hash;
            }
        }

        public bool Equals(XbmcVersion other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.Major, Major) && Equals(other.Minor, Minor) && Equals(other.Patch, Patch);
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

            if (obj.GetType() != typeof(XbmcVersion))
            {
                return false;
            }

            return Equals((XbmcVersion)obj);
        }

        public static XbmcVersion NONE = new XbmcVersion(0, 0, 0);
        public static XbmcVersion DHARMA = new XbmcVersion(2, 0, 0);
        public static XbmcVersion EDEN = new XbmcVersion(4, 0, 0);
        public static XbmcVersion FRODO = new XbmcVersion(6, 0, 0);
    }
}
