using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class QualityTypeProvider
    {
        private readonly IDatabase _database;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public QualityTypeProvider(IDatabase database)
        {
            _database = database;
        }

        public virtual void Update(QualityType qualityType)
        {
            _database.Update(qualityType);
        }

        public virtual void UpdateAll(List<QualityType> qualityTypes)
        {
            _database.UpdateMany(qualityTypes);
        }

        public virtual List<QualityType> All()
        {
            return _database.Fetch<QualityType>();
        }

        public virtual QualityType Get(int qualityTypeId)
        {
            return _database.Single<QualityType>(qualityTypeId);
        }

        public virtual List<QualityType> GetList(List<int> qualityTypeIds)
        {
            var queryParams = String.Join(", ", qualityTypeIds);
            var query = String.Format("WHERE QualityTypeId IN ({0})", queryParams);

            return _database.Fetch<QualityType>(query);
        }

        public virtual void SetupDefault()
        {
            if (All().Count != 0)
                return;

            Logger.Info("Setting up default quality types");

            var qualityTypes = new List<QualityType>();
            qualityTypes.Add(new QualityType { QualityTypeId = 1, Name = "SDTV", MinSize = 0, MaxSize = 10.Gigabytes() });
            qualityTypes.Add(new QualityType { QualityTypeId = 2, Name = "DVD", MinSize = 0, MaxSize = 10.Gigabytes() });
            qualityTypes.Add(new QualityType { QualityTypeId = 4, Name = "HDTV", MinSize = 0, MaxSize = 10.Gigabytes() });
            qualityTypes.Add(new QualityType { QualityTypeId = 5, Name = "WEBDL", MinSize = 0, MaxSize = 10.Gigabytes() });
            qualityTypes.Add(new QualityType { QualityTypeId = 6, Name = "Bluray720p", MinSize = 0, MaxSize = 10.Gigabytes() });
            qualityTypes.Add(new QualityType { QualityTypeId = 7, Name = "Bluray1080p", MinSize = 0, MaxSize = 10.Gigabytes() });

            _database.InsertMany(qualityTypes);
        }
    }
}
