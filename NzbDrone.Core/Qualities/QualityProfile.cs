using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class QualityProfile : ModelBase
    {
        public string Name { get; set; }
        public List<Quality> Allowed { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DbAllowed
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
                Allowed = new List<Quality>(qualities.Length);
                foreach (var quality in qualities.Where(q => !String.IsNullOrWhiteSpace(q)))
                {
                    Allowed.Add(Quality.FindById(Convert.ToInt32(quality)));
                }
            }
        }

        public Quality Cutoff { get; set; }
    }
}