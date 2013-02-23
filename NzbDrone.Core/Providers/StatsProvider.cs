using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class StatsProvider
    {
        private readonly IDatabase _database;

        public StatsProvider(IDatabase database)
        {
            _database = database;
        }

        public StatsProvider()
        {
        }

        public virtual StatsModel GetStats()
        {
            var series = _database.Fetch<Series>();
            var episodes = _database.Fetch<Episode>();
            var history = _database.Fetch<History.History>("WHERE Date >= @0", DateTime.Today.AddDays(-30));

            var stats = new StatsModel();
            stats.SeriesTotal = series.Count;
            stats.SeriesContinuing = series.Count(s => s.Status == "Continuing");
            stats.SeriesEnded = series.Count(s => s.Status == "Ended");
            stats.EpisodesTotal = episodes.Count;
            stats.EpisodesOnDisk = episodes.Count(e => e.EpisodeFileId > 0);
            stats.EpisodesMissing = episodes.Count(e => e.Ignored == false && e.EpisodeFileId == 0);
            stats.DownloadedLastMonth = history.Count;
            stats.DownloadLastWeek = history.Count(h => h.Date >= DateTime.Today.AddDays(-7));

            return stats;
        }
    }
}
