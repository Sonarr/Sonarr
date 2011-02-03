using System;
using System.Collections.Generic;
using System.ComponentModel;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository.Quality
{
    public class QualityProfile
    {
        [SubSonicPrimaryKey(true)]
        public int ProfileId { get; set; }
        public string Name { get; set; }
        public bool UserProfile { get; set; } //Allows us to tell the difference between default and user profiles

        public List<QualityTypes> Allowed { get; set; }
        public QualityTypes Cutoff { get; set; }


    }
}
