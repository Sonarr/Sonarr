using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class UpcomingEpisodesProvider
    {
        private readonly IRepository _repository;

        [Inject]
        public UpcomingEpisodesProvider(IRepository repository)
        {
            _repository = repository;
        }

        public virtual UpcomingEpisodesModel Upcoming()
        {
            var allEps =
                _repository.All<Episode>().Where(
                    e => e.AirDate >= DateTime.Today.AddDays(-1) && e.AirDate < DateTime.Today.AddDays(8));

            var yesterday = allEps.Where(e => e.AirDate == DateTime.Today.AddDays(-1)).ToList();
            var today = allEps.Where(e => e.AirDate == DateTime.Today).ToList();
            var week = allEps.Where(e => e.AirDate > DateTime.Today).ToList();

            return new UpcomingEpisodesModel { Yesterday = yesterday, Today = today, Week = week };
        }

        public virtual List<Episode> Yesterday()
        {
            return _repository.All<Episode>().Where(e => e.AirDate == DateTime.Today.AddDays(-1)).ToList();
        }

        public virtual List<Episode> Today()
        {
            return _repository.All<Episode>().Where(e => e.AirDate == DateTime.Today).ToList();
        }

        public virtual List<Episode> Tomorrow()
        {
            return _repository.All<Episode>().Where(e => e.AirDate == DateTime.Today.AddDays(1)).ToList();
        }

        public virtual List<Episode> Week()
        {
            return
                _repository.All<Episode>().Where(e => e.AirDate > DateTime.Today.AddDays(1) && e.AirDate < DateTime.Today.AddDays(8))
                    .ToList();
        }
    }
}