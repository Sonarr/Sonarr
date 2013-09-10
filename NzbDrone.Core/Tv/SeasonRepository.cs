using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;


namespace NzbDrone.Core.Tv
{
    public interface ISeasonRepository : IBasicRepository<Series>
    {
        Season Get(int seriesId, int seasonNumber);
        bool IsMonitored(int seriesId, int seasonNumber);
        List<Season> GetSeasonBySeries(int seriesId);
    }

    public class SeasonRepository : BasicRepository<Series>, ISeasonRepository
    {
        public SeasonRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }

        public Season Get(int seriesId, int seasonNumber)
        {
            var series = Query.Single(s => s.Id == seriesId);
            return series.Seasons.Single(s => s.SeasonNumber == seasonNumber);
        }

        public bool IsMonitored(int seriesId, int seasonNumber)
        {
            var series = Query.Single(s => s.Id == seriesId);
            return series.Seasons.Single(s => s.SeasonNumber == seasonNumber).Monitored;
        }

        public List<Season> GetSeasonBySeries(int seriesId)
        {
            return Query.Single(s => s.Id == seriesId).Seasons;
        }
    }
}