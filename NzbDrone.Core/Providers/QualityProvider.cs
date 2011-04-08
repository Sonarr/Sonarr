using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class QualityProvider
    {
        private IRepository _sonicRepo;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public QualityProvider()
        {
            
        }

        public QualityProvider(IRepository sonicRepo)
        {
            _sonicRepo = sonicRepo;
        }

        #region IQualityProvider Members

        public virtual void Add(QualityProfile profile)
        {
            _sonicRepo.Add(profile);
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

        #endregion
    }
}
