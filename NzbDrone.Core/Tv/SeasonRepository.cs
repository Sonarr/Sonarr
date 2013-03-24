using System.Collections.Generic;
using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;
using ServiceStack.OrmLite;

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
        private readonly IDbConnection _database;

        public SeasonRepository(IDbConnection database)
            : base(database)
        {
        }

        public IList<int> GetSeasonNumbers(int seriesId)
        {
            return _database.List<int>("SELECT SeasonNumber WHERE SeriesId = {0}", seriesId);
        }

        public Season Get(int seriesId, int seasonNumber)
        {
            return _database.Select<Season>(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber).Single();
        }

        public bool IsIgnored(int seriesId, int seasonNumber)
        {
            var season = _database.Select<Season>(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber).SingleOrDefault();

            if(season == null) return false;

            return season.Ignored;
        }

        public List<Season> GetSeasonBySeries(int seriesId)
        {
            return _database.Select<Season>(s =>  s.SeriesId == seriesId);
        }
    }
}