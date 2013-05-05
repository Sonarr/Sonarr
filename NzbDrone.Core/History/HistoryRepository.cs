using System;
using System.Data;
using System.Linq;
using NzbDrone.Common.Messaging;
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
    }
}