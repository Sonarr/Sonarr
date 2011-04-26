using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public class StatsProvider
    {
        private readonly SeriesProvider _seriesProvider;

        public StatsProvider(SeriesProvider seriesProvider)
        {
            _seriesProvider = seriesProvider;
        }

        public virtual int SeriesCount()
        {
            return _seriesProvider.GetAllSeries().Count();
        }

        public virtual int ActiveSeriesCount()
        {
            return _seriesProvider.GetAllSeries().Where(s => s.Status == "Continuing").Count();
        }

        public virtual int EndedSeriesCount()
        {
            return _seriesProvider.GetAllSeries().Where(s => s.Status == "Ended").Count();
        }

        public virtual int TotalEpisodesCount()
        {
            var count = 0;
            var series = _seriesProvider.GetAllSeries();
            foreach (var s in series)
            {
                count += s.Episodes.Count;
            }
            return count;
        }

        public virtual int TotalAiredEpisodesCount()
        {
            var count = 0;
            var series = _seriesProvider.GetAllSeries();
            foreach (var s in series)
            {
                count += s.Episodes.Where(e => e.AirDate.Date <= DateTime.Today).Count();
            }
            return count;
        }

        public virtual int TotalUnairedEpisodesCount()
        {
            var count = 0;
            var series = _seriesProvider.GetAllSeries();
            foreach (var s in series)
            {
                count += s.Episodes.Where(e => e.AirDate.Date > DateTime.Today).Count();
            }
            return count;
        }

        public virtual int TotalEpisodesOnDisk()
        {
            var count = 0;
            var series = _seriesProvider.GetAllSeries();
            foreach (var s in series)
            {
                count += s.Episodes.Where(e => e.EpisodeFileId != 0).Count();
            }
            return count;
        }


    }
}
