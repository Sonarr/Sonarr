using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using SubSonic.Repository;
using System.Linq;

namespace NzbDrone.Core.Providers
{
    public class SeasonProvider
    {
        private readonly IRepository _sonicRepo;
        private readonly SeriesProvider _seriesProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SeasonProvider(IRepository dataRepository, SeriesProvider seriesProvider)
        {
            _sonicRepo = dataRepository;
            _seriesProvider = seriesProvider;
        }

        public SeasonProvider()
        {

        }

        public virtual Season GetSeason(int seasonId)
        {
            return _sonicRepo.Single<Season>(seasonId);
        }

        public virtual Season GetSeason(int seriesId, int seasonNumber)
        {
            return _sonicRepo.Single<Season>(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber);
        }

        public virtual List<Season> GetSeasons(int seriesId)
        {
            return _sonicRepo.All<Season>().Where(s => s.SeriesId == seriesId).ToList();
        }

        public virtual Season GetLatestSeason(int seriesId)
        {
            return _sonicRepo.All<Season>().Where(s => s.SeriesId == seriesId).OrderBy(s => s.SeasonNumber).Last();
        }

        public virtual void EnsureSeason(int seriesId, int seasonId, int seasonNumber)
        {
            if (_sonicRepo.Exists<Season>(s => s.SeasonId == seasonId))
                return;
            //TODO: Calculate Season Folder
            Logger.Trace("Adding Season To DB. [SeriesID:{0} SeasonID:{1} SeasonNumber:{2}]", seriesId, seasonId, seasonNumber, "????");

            var newSeason = new Season()
            {
                Monitored = true,
                SeasonId = seasonId,
                SeasonNumber = seasonNumber,
                SeriesId = seriesId
            };
            _sonicRepo.Add<Season>(newSeason);
        }

        public virtual int SaveSeason(Season season)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsIgnored(int seasonId)
        {
            if (_sonicRepo.Single<Season>(seasonId).Monitored)
                return false;

            Logger.Debug("Season {0} is not wanted.");
            return true;
        }

        public virtual bool IsIgnored(int seriesId, int seasonNumber)
        {
            var season = _sonicRepo.Single<Season>(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber);

            if (season == null)
                return true;

            return !season.Monitored;
        }

        public void DeleteSeason(int seasonId)
        {
            _sonicRepo.Delete<Season>(seasonId);
        }
    }
}