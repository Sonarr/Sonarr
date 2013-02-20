using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class UpcomingEpisodesProvider
    {
        private readonly IDatabase _database;

        public UpcomingEpisodesProvider(IDatabase database)
        {
            _database = database;
        }

        //Todo: Might be best if this is part of episode repo (when its there)
        public virtual List<Episode> UpcomingEpisodes()
        {
            return _database.Fetch<Episode, Series>(@"SELECT * FROM Episodes 
                                                        INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                        WHERE Series.Monitored = 1 AND Ignored = 0 AND AirDate BETWEEN @0 AND @1"
                                                        ,DateTime.Today.AddDays(-1).Date, DateTime.Today.AddDays(8).Date);
        }
    }
}