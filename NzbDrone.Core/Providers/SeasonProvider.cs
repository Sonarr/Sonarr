using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Entities;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    class SeasonProvider : ISeasonProvider
    {
        private readonly IRepository _sonicRepo;
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public SeasonProvider(IRepository dataRepository)
        {
            _sonicRepo = dataRepository;
        }

        public Season GetSeason(int seasonId)
        {
            throw new NotImplementedException();
        }

        public List<Season> GetSeasongs(int seriesId)
        {
            throw new NotImplementedException();
        }

        public void EnsureSeason(int seriesId, int seasonId, int seasonNumber)
        {
            if (_sonicRepo.Exists<Season>(s => s.SeasonId == seasonId))
                return;
            //TODO: Calculate Season Folder
            Logger.Debug("Adding Season To DB. [SeriesID:{0} SeasonID:{1} SeasonNumber:{2} Folder:{3}]", seriesId, seasonId, seasonNumber, "????");

            var newSeason = new Season()
            {
                Folder = String.Empty,
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
    }
}