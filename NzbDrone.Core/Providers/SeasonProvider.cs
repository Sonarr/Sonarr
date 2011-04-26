using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class SeasonProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;

        public SeasonProvider(IRepository repository)
        {
            _repository = repository;
        }

        public SeasonProvider()
        {
        }

        public virtual Season GetSeason(int seasonId)
        {
            return _repository.Single<Season>(seasonId);
        }

        public virtual Season GetSeason(int seriesId, int seasonNumber)
        {
            return _repository.Single<Season>(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber);
        }

        public virtual List<Season> GetSeasons(int seriesId)
        {
            return _repository.All<Season>().Where(s => s.SeriesId == seriesId).ToList();
        }

        public virtual Season GetLatestSeason(int seriesId)
        {
            return _repository.All<Season>().Where(s => s.SeriesId == seriesId).OrderBy(s => s.SeasonNumber).Last();
        }

        public virtual void EnsureSeason(int seriesId, int seasonId, int seasonNumber)
        {
            if (_repository.Exists<Season>(s => s.SeasonId == seasonId))
                return;
            //TODO: Calculate Season Folder
            Logger.Trace("Adding Season To DB. [SeriesID:{0} SeasonID:{1} SeasonNumber:{2}]", seriesId, seasonId,
                         seasonNumber, "????");

            var newSeason = new Season
                                {
                                    Monitored = true,
                                    SeasonId = seasonId,
                                    SeasonNumber = seasonNumber,
                                    SeriesId = seriesId
                                };
            _repository.Add(newSeason);
        }

        public virtual int SaveSeason(Season season)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsIgnored(int seasonId)
        {
            if (_repository.Single<Season>(seasonId).Monitored)
                return false;

            Logger.Debug("Season {0} is not wanted.");
            return true;
        }

        public virtual bool IsIgnored(int seriesId, int seasonNumber)
        {
            var season = _repository.Single<Season>(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber);
            return !season.Monitored;
        }

        public void DeleteSeason(int seasonId)
        {
            _repository.Delete<Season>(seasonId);
        }
    }
}