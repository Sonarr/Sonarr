using System;
using System.Data;
using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;

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
                throw new InvalidOperationException("Sequence contains no element with qualityId = " + qualityId.ToString());
            }
        }
    }
}
