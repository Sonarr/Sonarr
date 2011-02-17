using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class QualityProvider : IQualityProvider
    {
        private IRepository _sonicRepo;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public QualityProvider(IRepository sonicRepo)
        {
            _sonicRepo = sonicRepo;
        }

        #region IQualityProvider Members

        public void Add(QualityProfile profile)
        {
            _sonicRepo.Add(profile);
        }

        public void Update(QualityProfile profile)
        {
            if (!_sonicRepo.Exists<QualityProfile>(q => q.QualityProfileId == profile.QualityProfileId))
            {
                Logger.Error("Unable to update non-existing profile");
                throw new InvalidOperationException("Unable to update non-existing profile");
            }

            _sonicRepo.Update(profile);
        }

        public void Delete(int profileId)
        {
            _sonicRepo.Delete<QualityProfile>(profileId);
        }

        public List<QualityProfile> GetAllProfiles()
        {
            var profiles = _sonicRepo.All<QualityProfile>().ToList();

            return profiles;
        }

        public QualityProfile Find(int profileId)
        {
            return _sonicRepo.Single<QualityProfile>(q => q.QualityProfileId == profileId);
        }

        #endregion
    }
}
