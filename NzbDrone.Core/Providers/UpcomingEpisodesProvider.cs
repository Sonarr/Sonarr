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
            return _database.Fetch<Episode, Series>(@"SELECT * FROM Episodes 
                                                        INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                        WHERE Ignored = 0 AND AirDate = @0", DateTime.Today.AddDays(-1));
        }

        public virtual List<Episode> Today()
        {
            return _database.Fetch<Episode, Series>(@"SELECT * FROM Episodes 
                                                        INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                        WHERE Ignored = 0 AND AirDate = @0", DateTime.Today);
        }

        public virtual List<Episode> Tomorrow()
        {
            return _database.Fetch<Episode, Series>(@"SELECT * FROM Episodes 
                                                        INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                        WHERE Ignored = 0 AND AirDate = @0", DateTime.Today.AddDays(1));
        }

        public virtual List<Episode> Week()
        {
            return _database.Fetch<Episode, Series>(@"SELECT * FROM Episodes 
                                                        INNER JOIN Series ON Episodes.SeriesId = Series.SeriesId
                                                        WHERE Ignored = 0 AND AirDate BETWEEN @0 AND @1", DateTime.Today.AddDays(2), DateTime.Today.AddDays(8));
        }
    }
}