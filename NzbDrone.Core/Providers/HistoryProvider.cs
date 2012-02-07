using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
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


        [Inject]
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

        public virtual Quality GetBestQualityInHistory(int seriesId, int seasonNumber, int episodeNumber)
        {
            var quality = _database.SingleOrDefault<dynamic>(@"SELECT TOP 1 History.Quality , History.IsProper FROM History 
                                                    INNER JOIN Episodes ON History.EpisodeId = Episodes.EpisodeId 
                                                    WHERE Episodes.seriesId = @0 AND 
                                                          Episodes.SeasonNumber = @1 AND  
                                                          Episodes.EpisodeNumber = @2
                                                    ORDER BY History.Quality DESC, History.IsProper DESC"
                                                    , seriesId, seasonNumber, episodeNumber);

            if (quality == null) return null;

            return new Quality((QualityTypes)quality.Quality, quality.IsProper);
        }

        public virtual void Delete(int historyId)
        {
            _database.Delete<History>(historyId);
        }
    }
}