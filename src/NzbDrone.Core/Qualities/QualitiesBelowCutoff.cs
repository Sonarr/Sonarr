using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Qualities
{
    public class QualitiesBelowCutoff
    {
        public Int32 ProfileId { get; set; }
        public IEnumerable<Int32> QualityIds { get; set; }

        public QualitiesBelowCutoff(int profileId, IEnumerable<int> qualityIds)
        {
            ProfileId = profileId;
            QualityIds = qualityIds;
        }
    }
}
