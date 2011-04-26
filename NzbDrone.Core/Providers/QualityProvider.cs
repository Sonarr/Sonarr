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
        private readonly IRepository _repository;

        public QualityProvider()
        {
        }

        public QualityProvider(IRepository repository)
        {
            _repository = repository;
        }

        public virtual int Add(QualityProfile profile)
        {
            return Convert.ToInt32(_repository.Add(profile));
        }

        public virtual void Update(QualityProfile profile)
        {
            if (!_repository.Exists<QualityProfile>(q => q.QualityProfileId == profile.QualityProfileId))
            {
                Logger.Error("Unable to update non-existing profile");
                throw new InvalidOperationException("Unable to update non-existing profile");
            }

            _repository.Update(profile);
        }

        public virtual void Delete(int profileId)
        {
            _repository.Delete<QualityProfile>(profileId);
        }

        public virtual List<QualityProfile> GetAllProfiles()
        {
            var profiles = _repository.All<QualityProfile>().ToList();

            return profiles;
        }

        public virtual QualityProfile Find(int profileId)
        {
            return _repository.Single<QualityProfile>(q => q.QualityProfileId == profileId);
        }
    }
}