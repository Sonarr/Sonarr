using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class QualityProfile
    {
        public int Id { get; set; }
        public Quality Cutoff { get; set; }

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
                Allowed = new List<Quality>(qualities.Length);
                foreach (var quality in qualities)
                {
                    Allowed.Add((Quality)Convert.ToInt32(quality));
                }
            }
        }

        [SubSonicIgnore]
        public List<Quality> Allowed { get; set; }
    }
}
