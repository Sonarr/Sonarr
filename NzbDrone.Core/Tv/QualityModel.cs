using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Tv
{
    public class QualityModel : IComparable<QualityModel>, IEmbeddedDocument
    {
        public Quality Quality { get; set; }

        public Boolean Proper { get; set; }

        public QualityModel()
            : this(Quality.Unknown)
        {

        }

        public QualityModel(Quality quality, Boolean proper = false)
        {
            Quality = quality;
            Proper = proper;
        }

        public int CompareTo(QualityModel other)
        {
            if (other.Quality > Quality)
                return -1;

            if (other.Quality < Quality)
                return 1;

            if (other.Quality == Quality && other.Proper == Proper)
                return 0;

            if (Proper && !other.Proper)
                return 1;

            if (!Proper && other.Proper)
                return -1;

            return 0;
        }

        public static bool operator !=(QualityModel x, QualityModel y)
        {
            return !(x == y);
        }

        public static bool operator ==(QualityModel x, QualityModel y)
        {
            var xObj = (Object)x;
            var yObj = (object)y;

            if (xObj == null || yObj == null)
            {
                return xObj == yObj;
            }

            return x.CompareTo(y) == 0;
        }

        public static bool operator >(QualityModel x, QualityModel y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <(QualityModel x, QualityModel y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator <=(QualityModel x, QualityModel y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >=(QualityModel x, QualityModel y)
        {
            return x.CompareTo(y) >= 0;
        }

        public override string ToString()
        {
            string result = Quality.ToString();
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
                hash = hash * 23 + Quality.GetHashCode();
                return hash;
            }
        }

        public bool Equals(QualityModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Quality, Quality) && other.Proper.Equals(Proper);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(QualityModel)) return false;
            return Equals((QualityModel)obj);
        }
    }
}
