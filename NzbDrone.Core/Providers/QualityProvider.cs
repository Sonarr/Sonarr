using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class QualityProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _sonicRepo;

        public QualityProvider()
        {
        }

        public QualityProvider(IRepository sonicRepo)
        {
            _sonicRepo = sonicRepo;
        }

        public virtual int Add(QualityProfile profile)
        {
            return Convert.ToInt32(_sonicRepo.Add(profile));
        }

        public virtual void Update(QualityProfile profile)
        {
            if (!_sonicRepo.Exists<QualityProfile>(q => q.QualityProfileId == profile.QualityProfileId))
            {
                Logger.Error("Unable to update non-existing profile");
                throw new InvalidOperationException("Unable to update non-existing profile");
            }

            _sonicRepo.Update(profile);
        }

        public virtual void Delete(int profileId)
        {
            _sonicRepo.Delete<QualityProfile>(profileId);
        }

        public virtual List<QualityProfile> GetAllProfiles()
        {
            var profiles = _sonicRepo.All<QualityProfile>().ToList();

            return profiles;
        }

        public virtual QualityProfile Find(int profileId)
        {
            return _sonicRepo.Single<QualityProfile>(q => q.QualityProfileId == profileId);
        }
    }
}