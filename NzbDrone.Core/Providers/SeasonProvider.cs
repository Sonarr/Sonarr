using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Repository;
using SubSonic.Repository;
using System.Linq;

namespace NzbDrone.Core.Providers
{
    class SeasonProvider : ISeasonProvider
    {
        private readonly IRepository _sonicRepo;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SeasonProvider(IRepository dataRepository)
        {
            _sonicRepo = dataRepository;
        }

        public Season GetSeason(int seasonId)
        {
            return _sonicRepo.Single<Season>(seasonId);
        }

        public List<Season> GetSeasons(int seriesId)
        {
            return _sonicRepo.All<Season>().Where(s => s.SeriesId == seriesId).ToList();
        }

        public Season GetLatestSeason(int seriesId)
        {
            return _sonicRepo.All<Season>().Where(s => s.SeriesId == seriesId).OrderBy(s => s.SeasonNumber).Last();
        }

        public void EnsureSeason(int seriesId, int seasonId, int seasonNumber)
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

        public int SaveSeason(Season season)
        {
            throw new NotImplementedException();
        }

        public bool IsIgnored(int seasonId)
        {
            if (_sonicRepo.Single<Season>(seasonId).Monitored)
                return true;

            Logger.Debug("Season {0} is not wanted.");
            return false;
        }

        public bool IsIgnored(int seriesId, int seasonNumber)
        {
            if (_sonicRepo.Single<Season>(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber).Monitored)
                return true;

            Logger.Debug("Season: {0} is not wanted for Series: {1}", seasonNumber, seriesId);
            return false;
        }
    }
}