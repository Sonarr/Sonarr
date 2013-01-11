using System;
using System.Collections.Generic;
using System.Linq;
using DataTables.Mvc.Core.Helpers;
using DataTables.Mvc.Core.Models;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class HistoryProvider
    {
        private readonly IDatabase _database;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        public HistoryProvider(IDatabase database)
        {
            _database = database;
        }

        public HistoryProvider()
        {
        }

        public virtual List<History> AllItems()
        {
            return _database.Fetch<History>("");
        }

        public virtual List<History> AllItemsWithRelationships()
        {
            return _database.Fetch<History, Episode>(@"
                            SELECT History.*, Series.Title as SeriesTitle, Episodes.* FROM History 
		                    INNER JOIN Series ON History.SeriesId = Series.SeriesId
                            INNER JOIN Episodes ON History.EpisodeId = Episodes.EpisodeId
                        ");
        }

        public virtual void Purge()
        {
            _database.Delete<History>("");
            logger.Info("History has been Purged");
        }

        public virtual void Trim()
        {
            _database.Delete<History>("WHERE Date < @0", DateTime.Now.AddDays(-30).Date);
            logger.Info("History has been trimmed, items older than 30 days have been removed");
        }

        public virtual void Add(History item)
        {
            _database.Insert(item);
            logger.Debug("Item added to history: {0}", item.NzbTitle);
        }

        public virtual QualityModel GetBestQualityInHistory(int seriesId, int seasonNumber, int episodeNumber)
        {
            var quality = _database.Fetch<QualityModel>(@"SELECT History.Quality , History.IsProper as Proper FROM History 
                                                          INNER JOIN Episodes ON History.EpisodeId = Episodes.EpisodeId 
                                                          WHERE Episodes.seriesId = @0 AND 
                                                          Episodes.SeasonNumber = @1 AND  
                                                          Episodes.EpisodeNumber = @2"
                                                    , seriesId, seasonNumber, episodeNumber);

            var best = quality.OrderByDescending(q => q).FirstOrDefault();

            return best;
        }

        public virtual void Delete(int historyId)
        {
            _database.Delete<History>(historyId);
        }

        public virtual Page<HistoryQueryModel> GetPagedItems(DataTablesPageRequest pageRequest)
        {
            var query = Sql.Builder
                    .Select(@"History.*, Series.Title as SeriesTitle, Episodes.Title as EpisodeTitle, 
                                Episodes.SeasonNumber as SeasonNumber, Episodes.EpisodeNumber as EpisodeNumber,
                                Episodes.Overview as EpisodeOverview")
                    .From("History")
                    .InnerJoin("Series")
                    .On("History.SeriesId = Series.SeriesId")
                    .InnerJoin("Episodes")
                    .On("History.EpisodeId = Episodes.EpisodeId");

            var startPage = (pageRequest.DisplayLength == 0) ? 1 : pageRequest.DisplayStart / pageRequest.DisplayLength + 1;

            if (!string.IsNullOrEmpty(pageRequest.Search))
            {
                var whereClause = string.Join(" OR ", SqlBuilderHelper.GetSearchClause(pageRequest));

                if (!string.IsNullOrEmpty(whereClause))
                    query.Append("WHERE " + whereClause, "%" + pageRequest.Search + "%");
            }

            var orderBy = string.Join(",", SqlBuilderHelper.GetOrderByClause(pageRequest));

            if (!string.IsNullOrEmpty(orderBy))
            {
                query.Append("ORDER BY " + orderBy);
            }

            return _database.Page<HistoryQueryModel>(startPage, pageRequest.DisplayLength, query);
        }

        public virtual long Count()
        {
            return _database.Single<long>(@"SELECT COUNT(*) from History");
        }
    }
}