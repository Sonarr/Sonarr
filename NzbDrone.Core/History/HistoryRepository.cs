using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<History>
    {
        void Trim();
        QualityModel GetBestQualityInHistory(int episodeId);
        PagingSpec<History> Paged(PagingSpec<History> pagingSpec);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {
        public HistoryRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }

        public void Trim()
        {
            var cutoff = DateTime.Now.AddDays(-30).Date;
            Delete(c=> c.Date < cutoff);
        }


        public QualityModel GetBestQualityInHistory(int episodeId)
        {
            var history = Query.Where(c => c.EpisodeId == episodeId)
                .OrderByDescending(c => c.Quality).FirstOrDefault();

            if (history != null)
            {
                return history.Quality;
            }

            return null;
        }

        public PagingSpec<History> Paged(PagingSpec<History> pagingSpec)
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