using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class UpcomingEpisodesProvider
    {
        private readonly IDatabase _database;

        [Inject]
        public UpcomingEpisodesProvider(IDatabase database)
        {
            _database = database;
        }

        public virtual List<Episode> Yesterday()
        {
            return RecentEpisodes().Where(c => c.AirDate.Value.Date == DateTime.Now.Date.AddDays(-1)).ToList();
        }

        public virtual List<Episode> Today()
        {
            return RecentEpisodes().Where(c => c.AirDate.Value.Date == DateTime.Now.Date).ToList();
        }

        public virtual List<Episode> Tomorrow()
        {
            return RecentEpisodes().Where(c => c.AirDate.Value.Date == DateTime.Now.Date.AddDays(1)).ToList();
        }

        public virtual List<Episode> Week()
        {
            return RecentEpisodes().Where(c => c.AirDate >= DateTime.Today.AddDays(2).Date).ToList();

        }

        private List<Episode> RecentEpisodes()
        {
            return _database.Fetch<Episode, Series>(@"SELECT * FROM Episodes 
                                                        INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                        WHERE Series.Monitored = 1 AND Ignored = 0 AND AirDate BETWEEN @0 AND @1"
                                                        ,DateTime.Today.AddDays(-1).Date, DateTime.Today.AddDays(8).Date);
        }
    }
}