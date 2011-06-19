using System;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Model
{
    public class Quality : IComparable<Quality>
    {
        public QualityTypes QualityType { get; set; }

        public Boolean Proper { get; set; }

        public Quality() { }

        public Quality(QualityTypes quality, Boolean proper)
        {
            QualityType = quality;
            Proper = proper;
        }

        public int CompareTo(Quality other)
        {
            if (other.QualityType > QualityType)
                return -1;

            if (other.QualityType < QualityType)
                return 1;

            if (other.QualityType == QualityType && other.Proper == Proper)
                return 0;

            if (Proper && !other.Proper)
                return 1;

            if (!Proper && other.Proper)
                return -1;

            return 0;
        }

        public static bool operator !=(Quality x, Quality y)
        {
            return !(x == y);
        }

        public static bool operator ==(Quality x, Quality y)
        {
            var xObj = (Object)x;
            var yObj = (object)y;

            if (xObj == null || yObj == null)
            {
                return xObj == yObj;
            }

            return x.CompareTo(y) == 0;
        }

        public static bool operator >(Quality x, Quality y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <(Quality x, Quality y)
        {
            return x.CompareTo(y) < 1;
        }

        public static bool operator <=(Quality x, Quality y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >=(Quality x, Quality y)
        {
            return x.CompareTo(y) >= 0;
        }

        public override string ToString()
        {
            string result = QualityType.ToString();
            if (Proper)
            {
                result += " [proper]";
            }

            return result;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Proper.GetHashCode();
                hash = hash * 23 + QualityType.GetHashCode();
                return hash;
            }
        }

        public bool Equals(Quality other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.QualityType, QualityType) && other.Proper.Equals(Proper);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Quality)) return false;
            return Equals((Quality) obj);
        }
    }
}
