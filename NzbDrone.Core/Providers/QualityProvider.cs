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

        public void AddProfile(QualityProfile profile, List<AllowedQuality> allowedQualities)
        {
            var profileId = _sonicRepo.Add(profile);

            foreach (var allowed in allowedQualities)
            {
                allowed.ProfileId = (int)profileId;
                _sonicRepo.Add<AllowedQuality>(allowed);
            }
        }

        public void UpdateProfile(QualityProfile profile, List<AllowedQuality> allowedQualities)
        {
            if (!_sonicRepo.Exists<QualityProfile>(q => q.ProfileId == profile.ProfileId))
            {
                //Log Error
                throw new NotImplementedException();
            }

            _sonicRepo.Update<QualityProfile>(profile);

            //Check to see if any items in the DB do not exist in this list
            //Check to see if any of the allowedQualities already exist, if so update, else add

            foreach (var inDb in _sonicRepo.All<AllowedQuality>().Where(q => q.ProfileId == profile.ProfileId))
            {
                if (!allowedQualities.Exists(l => l.ProfileId == inDb.ProfileId && l.Quality == inDb.Quality))
                    _sonicRepo.Delete<AllowedQuality>(inDb.Id);
            }

            foreach (var allowed in allowedQualities)
            {
                allowed.ProfileId = profile.ProfileId;
                if (!_sonicRepo.Exists<AllowedQuality>(q => q.ProfileId == profile.ProfileId && q.Quality == allowed.Quality))
                    _sonicRepo.Add(allowed);

                else
                    _sonicRepo.Update(allowed);
            }
        }

        public void RemoveProfile(int profileId)
        {
            _sonicRepo.DeleteMany<AllowedQuality>(q => q.ProfileId == profileId);
            _sonicRepo.Delete<QualityProfile>(profileId);
        }

        public List<QualityProfile> GetProfiles()
        {
            var profiles = _sonicRepo.All<QualityProfile>().ToList();

            foreach (var profile in profiles)
            {
                profile.AllowedQualities = _sonicRepo.Find<AllowedQuality>(q => q.ProfileId == profile.ProfileId).ToList();
            }
            return profiles;
        }

        #endregion
    }
}
