using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Qualities
{
    public class Quality : IComparable<Quality>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Weight { get; set; }

        public int CompareTo(Quality other)
        {
            if (other.Weight > Weight)
                return -1;

            if (other.Weight < Weight)
                return 1;

            if (other.Weight == Weight)
                return 0;

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
            return x.CompareTo(y) < 0;
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

        public bool Equals(Quality other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Weight, Weight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Quality)) return false;
            return Equals((Quality) obj);
        }

        public static readonly Quality Unknown = new Quality { Id = 0, Name = "Unknown", Weight = 0 };
        public static readonly Quality SDTV = new Quality {Id = 1, Name = "SDTV", Weight = 1};
        public static readonly Quality WEBDL480p = new Quality { Id = 8, Name = "WEBDL-480p", Weight = 2 };
        public static readonly Quality DVD = new Quality { Id = 2, Name = "DVD", Weight = 3 };
        public static readonly Quality HDTV720p = new Quality { Id = 4, Name = "HDTV-720p", Weight = 4 };
        public static readonly Quality HDTV1080p = new Quality { Id = 9, Name = "HDTV-1080p", Weight = 5 };
        public static readonly Quality RAWHD = new Quality { Id = 10, Name = "Raw-HD", Weight = 6 };
        public static readonly Quality WEBDL720p = new Quality { Id = 5, Name = "WEBDL-720p", Weight = 7 };
        public static readonly Quality Bluray720p = new Quality { Id = 6, Name = "Bluray720p", Weight = 8 };
        public static readonly Quality WEBDL1080p = new Quality { Id = 3, Name = "WEBDL-1080p", Weight = 9 };
        public static readonly Quality Bluray1080p = new Quality { Id = 7, Name = "Bluray1080p", Weight = 10 };


        public static List<Quality> All()
        {
            return new List<Quality>
                       {
                               Unknown,
                               SDTV,
                               WEBDL480p,
                               DVD,
                               HDTV720p,
                               HDTV1080p,
                               RAWHD,
                               WEBDL720p,
                               WEBDL1080p,
                               Bluray720p,
                               Bluray1080p
                       };
        }

        public static Quality FindById(int id)
        {
            var quality = All().SingleOrDefault(q => q.Id == id);

            if (quality == null)
                throw new ArgumentException("ID does not match a known quality", "id");

            return quality;            
        }

        public static explicit operator Quality(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(Quality quality)
        {
            return quality.Id;
        }
    }
}