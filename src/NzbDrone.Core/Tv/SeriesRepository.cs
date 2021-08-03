using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesRepository : IBasicRepository<Series>
    {
        bool SeriesPathExists(string path);
        Series FindByTitle(string cleanTitle);
        Series FindByTitle(string cleanTitle, int year);
        List<Series> FindByTitleInexact(string cleanTitle);
        Series FindByTvdbId(int tvdbId);
        Series FindByTvRageId(int tvRageId);
        Series FindByPath(string path);
        List<string> AllSeriesPaths();
    }

    public class SeriesRepository : BasicRepository<Series>, ISeriesRepository
    {
        protected IMainDatabase _database;

        public SeriesRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _database = database;
        }

        public bool SeriesPathExists(string path)
        {
            return Query.Where(c => c.Path == path).Any();
        }

        public Series FindByTitle(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = Query.Where(s => s.CleanTitle == cleanTitle)
                                        .ToList();

            return ReturnSingleSeriesOrThrow(series);
        }

        public Series FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = Query.Where(s => s.CleanTitle == cleanTitle)
                                        .AndWhere(s => s.Year == year)
                                        .ToList();

            return ReturnSingleSeriesOrThrow(series);
        }

        public List<Series> FindByTitleInexact(string cleanTitle)
        {
            var mapper = _database.GetDataMapper();
            mapper.AddParameter("CleanTitle", cleanTitle);

            return mapper.Query<Series>().Where($"instr(@CleanTitle, [t0].[CleanTitle])");
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return Query.Where(s => s.TvdbId == tvdbId).SingleOrDefault();
        }

        public Series FindByTvRageId(int tvRageId)
        {
            return Query.Where(s => s.TvRageId == tvRageId).SingleOrDefault();
        }

        public Series FindByPath(string path)
        {
            return Query.Where(s => s.Path == path)
                        .FirstOrDefault();
        }

        public List<string> AllSeriesPaths()
        {
            var mapper = _database.GetDataMapper();

            return mapper.Query<string>("SELECT Path from Series");
        }

        private Series ReturnSingleSeriesOrThrow(List<Series> series)
        {
            if (series.Count == 0)
            {
                return null;
            }

            if (series.Count == 1)
            {
                return series.First();
            }

            throw new MultipleSeriesFoundException("Expected one series, but found {0}. Matching series: {1}", series.Count, string.Join(",", series));
        }
    }
}
