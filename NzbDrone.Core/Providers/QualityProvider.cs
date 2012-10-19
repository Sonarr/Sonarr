using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class QualityProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;

        public QualityProvider()
        {
        }

        [Inject]
        public QualityProvider(IDatabase database)
        {
            _database = database;
        }

        public virtual int Add(QualityProfile profile)
        {
            return Convert.ToInt32(_database.Insert(profile));
        }

        public virtual void Update(QualityProfile profile)
        {
            if (!_database.Exists<QualityProfile>("WHERE QualityProfileid = @0", profile.QualityProfileId))
            {
                Logger.Error("Unable to update non-existing profile");
                throw new InvalidOperationException("Unable to update non-existing profile");
            }

            _database.Update(profile);
        }

        public virtual void Delete(int profileId)
        {
            _database.Delete<QualityProfile>(profileId);
        }

        public virtual List<QualityProfile> All()
        {
            var profiles = _database.Fetch<QualityProfile>().ToList();

            return profiles;
        }

        public virtual QualityProfile Get(int profileId)
        {
            return _database.Single<QualityProfile>(profileId);
        }

        public virtual void SetupDefaultProfiles()
        {
            if (All().Count != 0)
                return;

            Logger.Info("Setting up default quality profiles");

            var sd = new QualityProfile { Name = "SD", Allowed = new List<QualityTypes> { QualityTypes.SDTV, QualityTypes.DVD }, Cutoff = QualityTypes.SDTV };

            var hd = new QualityProfile
            {
                Name = "HD",
                Allowed = new List<QualityTypes> { QualityTypes.HDTV, QualityTypes.WEBDL720p, QualityTypes.Bluray720p },
                Cutoff = QualityTypes.HDTV
            };

            Add(sd);
            Add(hd);

        }
    }
}