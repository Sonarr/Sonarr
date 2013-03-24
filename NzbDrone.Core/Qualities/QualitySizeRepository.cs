using System.Data;
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
        public QualitySizeRepository(IDbConnection database)
            : base(database)
        {
        }

        public QualitySize GetByQualityId(int qualityId)
        {
            return Single(q => q.QualityId == qualityId);
        }
    }
}
