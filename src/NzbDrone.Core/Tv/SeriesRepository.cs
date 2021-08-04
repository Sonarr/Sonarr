using System.Collections.Generic;
using System.Linq;
using Dapper;
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
        Dictionary<int, string> AllSeriesPaths();
    }

    public class SeriesRepository : BasicRepository<Series>, ISeriesRepository
    {
        public SeriesRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool SeriesPathExists(string path)
        {
            return Query(c => c.Path == path).Any();
        }

        public Series FindByTitle(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = Query(s => s.CleanTitle == cleanTitle)
                                        .ToList();

            return ReturnSingleSeriesOrThrow(series);
        }

        public Series FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = Query(s => s.CleanTitle == cleanTitle && s.Year == year).ToList();

            return ReturnSingleSeriesOrThrow(series);
        }

        public List<Series> FindByTitleInexact(string cleanTitle)
        {
            var builder = Builder().Where($"instr(@cleanTitle, Series.[CleanTitle])", new { cleanTitle = cleanTitle });

            return Query(builder).ToList();
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return Query(s => s.TvdbId == tvdbId).SingleOrDefault();
        }

        public Series FindByTvRageId(int tvRageId)
        {
            return Query(s => s.TvRageId == tvRageId).SingleOrDefault();
        }

        public Series FindByPath(string path)
        {
            return Query(s => s.Path == path)
                        .FirstOrDefault();
        }

        public Dictionary<int, string> AllSeriesPaths()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT Id AS [Key], Path AS [Value] FROM Series";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
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
