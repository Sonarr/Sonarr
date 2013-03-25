using System;
using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<History>
    {
        void Trim();
        QualityModel GetBestQualityInHistory(int episodeId);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {
        public HistoryRepository(IDatabase database)
            : base(database)
        {
        }

        public void Trim()
        {
            var oldIds =  Queryable().Where(c => c.Date < DateTime.Now.AddDays(-30).Date).Select(c => c.Id);
            DeleteMany(oldIds);
        }


        public QualityModel GetBestQualityInHistory(int episodeId)
        {
            var history = Queryable().Where(c => c.EpisodeId == episodeId)
                .OrderByDescending(c => c.Quality).FirstOrDefault();

            if (history != null)
            {
                return history.Quality;
            }

            return null;
        }
    }
}