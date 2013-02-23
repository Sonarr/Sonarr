using System;
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
        public HistoryRepository(IObjectDatabase objectDatabase)
                : base(objectDatabase)
        {
        }

        public void Trim()
        {
            var oldIds = Queryable.Where(c => c.Date < DateTime.Now.AddDays(-30).Date).Select(c => c.OID);
            DeleteMany(oldIds);
        }


        public QualityModel GetBestQualityInHistory(int seriesId, int seasonNumber, int episodeNumber)
        {
            var history = Queryable.OrderByDescending(c => c.Quality).FirstOrDefault(c => c.Episode.Series.SeriesId == seriesId && c.Episode.SeasonNumber == seasonNumber &&
                                                                                          c.Episode.EpisodeNumber == episodeNumber);

            if (history != null)
            {
                return history.Quality;
            }

            return null;
        }
    }
}