using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Tv
{
    public interface ISeasonRepository : IBasicRepository<Series>
    {
        List<Season> GetSeasonBySeries(int seriesId);
    }

    public class SeasonRepository : BasicRepository<Series>, ISeasonRepository
    {
        public SeasonRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Season> GetSeasonBySeries(int seriesId)
        {
            return Query.Single(s => s.Id == seriesId).Seasons;
        }
    }
}