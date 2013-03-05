using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public interface IQualitySizeRepository : IBasicRepository<QualitySize>
    {
        QualitySize GetByQualityId(int qualityId);
    }

    public class QualitySizeRepository : BasicRepository<QualitySize>, IQualitySizeRepository
    {
        public QualitySizeRepository(IObjectDatabase database)
            : base(database)
        {
        }

        public QualitySize GetByQualityId(int qualityId)
        {
            return Queryable.Single(q => q.QualityId == qualityId);
        }
    }
}
