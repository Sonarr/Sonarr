using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;


namespace NzbDrone.Core.Tv
{
    public interface ISeasonRepository : IBasicRepository<Season>
    {
        IList<int> GetSeasonNumbers(int seriesId);
        Season Get(int seriesId, int seasonNumber);
        bool IsMonitored(int seriesId, int seasonNumber);
        List<Season> GetSeasonBySeries(int seriesId);
    }

    public class SeasonRepository : BasicRepository<Season>, ISeasonRepository
    {

        public SeasonRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }

        public IList<int> GetSeasonNumbers(int seriesId)
        {
            return Query.Where(c => c.SeriesId == seriesId).Select(c => c.SeasonNumber).ToList();
        }

        public Season Get(int seriesId, int seasonNumber)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber);
        }

        public bool IsMonitored(int seriesId, int seasonNumber)
        {
            var season = Query.SingleOrDefault(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber);

            if (season == null) return true;

            return season.Monitored;
        }

        public List<Season> GetSeasonBySeries(int seriesId)
        {
            return Query.Where(s => s.SeriesId == seriesId);
        }
    }
}