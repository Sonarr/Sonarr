using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PetaPoco;

namespace NzbDrone.Core.Repository.Quality
{
    [TableName("QualityProfiles")]
    [PrimaryKey("QualityProfileId", autoIncrement = true)]
    public class QualityProfile
    {
        public virtual int QualityProfileId { get; set; }

        [Required(ErrorMessage = "A Name is Required")]
        [DisplayName("Name")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        [Ignore]
        [DisplayName("Allowed Qualities")]
        public List<QualityTypes> Allowed { get; set; }

        [Ignore]
        [DisplayName("Allowed Qualities String")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AllowedString { get; set; }

        [DisplayName("Cut-off")]
        [Required(ErrorMessage = "Valid Cut-off is Required")]
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
                    result += (int)q + "|";
                }
                return result.Trim('|');
            }
            private set
            {
                var qualities = value.Split('|');
                Allowed = new List<QualityTypes>(qualities.Length);
                foreach (var quality in qualities.Where(q => !String.IsNullOrWhiteSpace(q)))
                {
                    Allowed.Add((QualityTypes)Convert.ToInt32(quality));
                }
            }
        }
    }
}