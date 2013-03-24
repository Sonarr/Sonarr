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
        QualityModel GetBestQualityInHistory(int seriesId, int seasonNumber, int episodeNumber);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {
        public HistoryRepository(IDbConnection database)
            : base(database)
        {
        }

        public void Trim()
        {
            var oldIds = Where(c => c.Date < DateTime.Now.AddDays(-30).Date).Select(c => c.Id);
            DeleteMany(oldIds);
        }


        public QualityModel GetBestQualityInHistory(int seriesId, int seasonNumber, int episodeNumber)
        {
            var history = Where(c => c.Episode.Series.Id == seriesId && c.Episode.SeasonNumber == seasonNumber && c.Episode.EpisodeNumber == episodeNumber)
                .OrderByDescending(c => c.Quality).FirstOrDefault();

            if (history != null)
            {
                return history.Quality;
            }

            return null;
        }
    }
}