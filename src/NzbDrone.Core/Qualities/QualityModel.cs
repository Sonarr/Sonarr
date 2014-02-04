using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Qualities
{
    public class QualityModel : IEmbeddedDocument, IEquatable<QualityModel>
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


        public override string ToString()
        {
            string result = Quality.ToString();
            if (Proper)
            {
                result += " Proper";
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
            return other.Quality.Equals(Quality) && other.Proper.Equals(Proper);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return Equals(obj as QualityModel);
        }

        public static bool operator ==(QualityModel left, QualityModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(QualityModel left, QualityModel right)
        {
            return !Equals(left, right);
        }
    }
}
