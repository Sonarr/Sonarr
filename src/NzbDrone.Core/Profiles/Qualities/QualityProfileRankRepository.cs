using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Qualities
{
    public interface IQualityProfileRankRepository : IBasicRepository<QualityProfileQualityRank>
    {
        void ReplaceForProfile(int profileId, IEnumerable<QualityProfileQualityRank> ranks);
        void DeleteForProfile(int profileId);
    }

    public class QualityProfileRankRepository : BasicRepository<QualityProfileQualityRank>, IQualityProfileRankRepository
    {
        public QualityProfileRankRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void ReplaceForProfile(int profileId, IEnumerable<QualityProfileQualityRank> ranks)
        {
            DeleteForProfile(profileId);

            var toInsert = ranks.ToList();

            if (toInsert.Count == 0)
            {
                return;
            }

            InsertMany(toInsert);
        }

        public void DeleteForProfile(int profileId)
        {
            Delete(r => r.ProfileId == profileId);
        }
    }
}
