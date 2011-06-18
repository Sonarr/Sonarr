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

        public virtual List<QualityProfile> GetAllProfiles()
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
            Logger.Info("Setting up default quality profiles");

            var profiles = GetAllProfiles();

            var sd = new QualityProfile { Name = "SD", Allowed = new List<QualityTypes> { QualityTypes.SDTV, QualityTypes.DVD }, Cutoff = QualityTypes.SDTV };

            var hd = new QualityProfile
            {
                Name = "HD",
                Allowed = new List<QualityTypes> { QualityTypes.HDTV, QualityTypes.WEBDL, QualityTypes.Bluray720p },
                Cutoff = QualityTypes.HDTV
            };

            //Add or Update SD
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", sd.Name));
            var sdDb = profiles.Where(p => p.Name == sd.Name).FirstOrDefault();
            if (sdDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", sd.Name));
                Add(sd);
            }

            //Add or Update HD
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", hd.Name));
            var hdDb = profiles.Where(p => p.Name == hd.Name).FirstOrDefault();
            if (hdDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", hd.Name));
                Add(hd);
            }
        }
    }
}