using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PetaPoco;

namespace NzbDrone.Core.Repository.Quality
{
    [TableName("QualityProfiles")]
    [PrimaryKey("QualityProfileId", autoIncrement = true)]
    public class QualityProfile
    {
        public virtual int QualityProfileId { get; set; }
        public string Name { get; set; }

        [Ignore]
        public List<QualityTypes> Allowed { get; set; }

        [Ignore]
        public string AllowedString { get; set; }
        public QualityTypes Cutoff { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string SonicAllowed
        {
            get
            {
                string result = String.Empty;
                if (Allowed == null) return result;

                foreach (var q in Allowed)
                {
                    result += q.Id + "|";
                }
                return result.Trim('|');
            }
            private set
            {
                var qualities = value.Split('|');
                Allowed = new List<QualityTypes>(qualities.Length);
                foreach (var quality in qualities.Where(q => !String.IsNullOrWhiteSpace(q)))
                {
                    Allowed.Add(QualityTypes.FindById(Convert.ToInt32(quality)));
                }
            }
        }
    }
}