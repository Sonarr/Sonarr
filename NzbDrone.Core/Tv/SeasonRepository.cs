using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;


namespace NzbDrone.Core.Tv
{
    public interface ISeasonRepository : IBasicRepository<Series>
    {
        List<Season> GetSeasonBySeries(int seriesId);
    }

    public class SeasonRepository : BasicRepository<Series>, ISeasonRepository
    {
        public SeasonRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }

        public List<Season> GetSeasonBySeries(int seriesId)
        {
            return Query.Single(s => s.Id == seriesId).Seasons;
        }
    }
}