using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class UpcomingEpisodesProvider : IUpcomingEpisodesProvider
    {
        private IRepository _sonicRepo;

        public UpcomingEpisodesProvider(IRepository sonicRepo)
        {
            _sonicRepo = sonicRepo;
        }

        #region IUpcomingEpisodesProvider

        public UpcomingEpisodesModel Upcoming()
        {
            var allEps = _sonicRepo.All<Episode>().Where(e => e.AirDate >= DateTime.Today.AddDays(-1) && e.AirDate < DateTime.Today.AddDays(8));

            var yesterday = allEps.Where(e => e.AirDate == DateTime.Today.AddDays(-1)).ToList();
            var today = allEps.Where(e => e.AirDate == DateTime.Today).ToList();
            var week = allEps.Where(e => e.AirDate > DateTime.Today).ToList();

            return new UpcomingEpisodesModel {Yesterday = yesterday, Today = today, Week = week};
        }

        public List<Episode> Yesterday()
        {
            return _sonicRepo.All<Episode>().Where(e => e.AirDate == DateTime.Today.AddDays(-1)).ToList();
        }

        public List<Episode> Today()
        {
            return _sonicRepo.All<Episode>().Where(e => e.AirDate == DateTime.Today).ToList();
        }

        public List<Episode> Week()
        {
            return _sonicRepo.All<Episode>().Where(e => e.AirDate > DateTime.Today && e.AirDate < DateTime.Today.AddDays(8)).ToList();
        }

        #endregion
    }
}
