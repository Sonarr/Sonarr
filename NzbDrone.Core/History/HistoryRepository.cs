using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<History>
    {
        void Trim();
        List<QualityModel> GetEpisodeHistory(int episodeId);
        void CleanupOrphanedBySeries();
        void CleanupOrphanedByEpisode();
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {
        private readonly IDatabase _database;

        public HistoryRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _database = database;
        }

        public void Trim()
        {
            var cutoff = DateTime.UtcNow.AddDays(-30).Date;
            Delete(c=> c.Date < cutoff);
        }

        public List<QualityModel> GetEpisodeHistory(int episodeId)
        {
            var history = Query.Where(c => c.EpisodeId == episodeId);

            return history.Select(h => h.Quality).ToList();
        }

        public void CleanupOrphanedBySeries()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM History
                                     WHERE Id IN (
                                     SELECT History.Id FROM History
                                     LEFT OUTER JOIN Series
                                     ON History.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL)");
        }

        public void CleanupOrphanedByEpisode()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM History
                                     WHERE Id IN (
                                     SELECT History.Id FROM History
                                     LEFT OUTER JOIN Episodes
                                     ON History.EpisodeId = Episodes.Id
                                     WHERE Episodes.Id IS NULL)");
        }

        public override PagingSpec<History> GetPaged(PagingSpec<History> pagingSpec)
        {
            var pagingQuery = Query.Join<History, Series>(JoinType.Inner, h => h.Series, (h, s) => h.SeriesId == s.Id)
                                   .Join<History, Episode>(JoinType.Inner, h => h.Episode, (h, e) => h.EpisodeId == e.Id)
                                   .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                                   .Skip(pagingSpec.PagingOffset())
                                   .Take(pagingSpec.PageSize);

            pagingSpec.Records = pagingQuery.ToList();

            //TODO: Use the same query for count and records
            pagingSpec.TotalRecords = Count();

            return pagingSpec;
        }
    }
}