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

        public QualityTypeProvider()
        {
            
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
            var inDb = All();

            Logger.Debug("Setting up default quality types");

            foreach(var qualityType in QualityTypes.All())
            {
                //Skip UNKNOWN
                if (qualityType.Id == 0) continue;

                var db = inDb.SingleOrDefault(s => s.QualityTypeId == qualityType.Id);

                if (db == null)
                    _database.Insert(new QualityType { QualityTypeId = qualityType.Id, Name = qualityType.Name, MinSize = 0, MaxSize = 100 });
            }
        }
    }
}
