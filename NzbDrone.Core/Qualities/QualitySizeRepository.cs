using System;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging;


namespace NzbDrone.Core.Qualities
{
    public interface IQualitySizeRepository : IBasicRepository<QualitySize>
    {
        QualitySize GetByQualityId(int qualityId);
    }

    public class QualitySizeRepository : BasicRepository<QualitySize>, IQualitySizeRepository
    {
        public QualitySizeRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }

        public QualitySize GetByQualityId(int qualityId)
        {
            try
            {
                return Query.Single(q => q.QualityId == qualityId);
            }
            catch (InvalidOperationException e)
            {
                throw new ModelNotFoundException(typeof(QualitySize), qualityId);
            }
        }
    }
}
