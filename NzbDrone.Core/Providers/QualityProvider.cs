using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class QualityProvider : IQualityProvider
    {
        private IRepository _sonicRepo;

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
            if (!_sonicRepo.Exists<QualityProfile>(q => q.ProfileId == profile.ProfileId))
            {
                //Log Error
                throw new InvalidOperationException("Unable to update none existing profile");
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

        #endregion
    }
}
