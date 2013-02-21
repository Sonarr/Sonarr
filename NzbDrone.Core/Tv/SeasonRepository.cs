using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using PetaPoco;

namespace NzbDrone.Core.Tv
{
    public interface ISeasonRepository : IBasicRepository<Season>
    {
        IList<int> GetSeasonNumbers(int seriesId);
        Season Get(int seriesId, int seasonNumber);
        bool IsIgnored(int seriesId, int seasonNumber);
        List<Season> GetSeasonBySeries(int seriesId);
    }

    public class SeasonRepository : BasicRepository<Season>, ISeasonRepository
    {
        public SeasonRepository(IObjectDatabase database)
                : base(database)
        {
        }

        public IList<int> GetSeasonNumbers(int seriesId)
        {
            return Queryable.Where(c => c.SeriesId == seriesId).Select(c => c.SeasonNumber).ToList();
        }

        public Season Get(int seriesId, int seasonNumber)
        {
            return Queryable.Single(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber);
        }

        public bool IsIgnored(int seriesId, int seasonNumber)
        {
            return Queryable.Single(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber).Ignored;
        }

        public List<Season> GetSeasonBySeries(int seriesId)
        {
            return Queryable.Where(c => c.SeriesId == seriesId).ToList();
        }

    }
}