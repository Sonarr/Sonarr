using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesTranslationRepository : IBasicRepository<SeriesTranslation>
    {
        List<SeriesTranslation> GetTranslations(int seriesId);
        void DeleteTranslations(List<int> seriesIds);
    }

    public class SeriesTranslationRepository : BasicRepository<SeriesTranslation>, ISeriesTranslationRepository
    {
        public SeriesTranslationRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<SeriesTranslation> GetTranslations(int seriesId)
        {
            return Query(c => c.SeriesId == seriesId);
        }

        public void DeleteTranslations(List<int> seriesIds)
        {
            Delete(t => seriesIds.Contains(t.SeriesId));
        }
    }
}
