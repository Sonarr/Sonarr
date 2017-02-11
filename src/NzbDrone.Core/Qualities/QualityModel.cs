using System;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class QualityModel : IEmbeddedDocument, IEquatable<QualityModel>, IComparable
    {
        public Quality Quality { get; set; }
        public Revision Revision { get; set; }

        [JsonIgnore]
        public QualitySource QualitySource { get; set; }
        
        public QualityModel()
            : this(Quality.Unknown, new Revision())
        {

        }

        public QualityModel(Quality quality, Revision revision = null)
        {
            Quality = quality;
            Revision = revision ?? new Revision();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Quality, Revision);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Revision.GetHashCode();
                hash = hash * 23 + Quality.GetHashCode();
                return hash;
            }
        }

        public int CompareTo(object obj)
        {
            var other = (QualityModel) obj;
            var definition = Quality.DefaultQualityDefinitions.First(q => q.Quality == Quality);
            var otherDefinition = Quality.DefaultQualityDefinitions.First(q => q.Quality == other.Quality);

            if (definition.Weight > otherDefinition.Weight)
            {
                return 1;
            }

            if(definition.Weight < otherDefinition.Weight)
            {
                return -1;
            }

            if (Revision.Real > other.Revision.Real)
            {
                return 1;
            }

            if (Revision.Real < other.Revision.Real)
            {
                return -1;
            }

            if (Revision.Version > other.Revision.Version)
            {
                return 1;
            }

            if (Revision.Version < other.Revision.Version)
            {
                return -1;
            }

            return 0;
        }

        public bool Equals(QualityModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.Quality.Equals(Quality) && other.Revision.Equals(Revision);
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
