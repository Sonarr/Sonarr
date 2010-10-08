using System;
using System.Collections.Generic;
using System.ComponentModel;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Entities.Quality
{
    public class QualityProfile
    {
        public int Id { get; set; }
        public QualityTypes Cutoff { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string SonicAllowed
        {
            get
            {
                string result = String.Empty;
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
                foreach (var quality in qualities)
                {
                    Allowed.Add((QualityTypes)Convert.ToInt32(quality));
                }
            }
        }

        [SubSonicIgnore]
        public List<QualityTypes> Allowed { get; set; }
    }
}
