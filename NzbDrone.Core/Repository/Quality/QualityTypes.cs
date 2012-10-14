using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Repository.Quality
{
    public class QualityTypes : IComparable<QualityTypes>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Weight { get; set; }

        public int CompareTo(QualityTypes other)
        {
            if (other.Weight > Weight)
                return -1;

            if (other.Weight < Weight)
                return 1;

            if (other.Weight == Weight)
                return 0;

            return 0;
        }

        public static bool operator !=(QualityTypes x, QualityTypes y)
        {
            return !(x == y);
        }

        public static bool operator ==(QualityTypes x, QualityTypes y)
        {
            var xObj = (Object)x;
            var yObj = (object)y;

            if (xObj == null || yObj == null)
            {
                return xObj == yObj;
            }

            return x.CompareTo(y) == 0;
        }

        public static bool operator >(QualityTypes x, QualityTypes y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <(QualityTypes x, QualityTypes y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator <=(QualityTypes x, QualityTypes y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >=(QualityTypes x, QualityTypes y)
        {
            return x.CompareTo(y) >= 0;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Weight.GetHashCode();
                return hash;
            }
        }

        public bool Equals(QualityTypes other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Weight, Weight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (QualityTypes)) return false;
            return Equals((QualityTypes) obj);
        }

        public static QualityTypes Unknown = new QualityTypes { Id = 0, Name = "Unknown", Weight = 0 };
        public static QualityTypes SDTV = new QualityTypes {Id = 1, Name = "SDTV", Weight = 1};
        public static QualityTypes DVD = new QualityTypes { Id = 2, Name = "DVD", Weight = 2 };
        public static QualityTypes HDTV = new QualityTypes { Id = 4, Name = "HDTV", Weight = 4 };
        public static QualityTypes WEBDL = new QualityTypes { Id = 5, Name = "WEBDL", Weight = 5 };
        public static QualityTypes Bluray720p = new QualityTypes { Id = 6, Name = "Bluray720p", Weight = 6 };
        public static QualityTypes Bluray1080p = new QualityTypes { Id = 7, Name = "Bluray1080p", Weight = 7 };

        public static List<QualityTypes> All()
        {
            return new List<QualityTypes>
                       {
                               Unknown,
                               SDTV,
                               DVD,
                               HDTV,
                               WEBDL,
                               Bluray720p,
                               Bluray1080p
                       };
        }

        public static QualityTypes FindById(int id)
        {
            var quality = All().SingleOrDefault(q => q.Id == id);

            if (quality == null)
                throw new ArgumentException("ID does not match a known quality", "id");

            return quality;            
        }

        public static explicit operator QualityTypes(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(QualityTypes quality)
        {
            return quality.Id;
        }
    }
}