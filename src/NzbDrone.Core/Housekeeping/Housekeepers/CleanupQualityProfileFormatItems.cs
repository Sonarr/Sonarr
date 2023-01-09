using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupQualityProfileFormatItems : IHousekeepingTask
    {
        private readonly IMainDatabase _database;
        private readonly IQualityProfileFormatItemsCleanupRepository _repository;

        public CleanupQualityProfileFormatItems(IMainDatabase database, IQualityProfileFormatItemsCleanupRepository repository)
        {
            _database = database;
            _repository = repository;
        }

        public void Clean()
        {
            var customFormatIds = GetCustomFormatIds();
            var profiles = _repository.All();
            var updatedProfiles = new List<QualityProfile>();

            foreach (var profile in profiles)
            {
                var formatItems = profile.FormatItems.Where(f => customFormatIds.Contains(f.Id)).ToList();

                if (formatItems.Count != profile.FormatItems.Count)
                {
                    profile.FormatItems = formatItems;

                    if (profile.FormatItems.Empty())
                    {
                        profile.MinFormatScore = 0;
                        profile.CutoffFormatScore = 0;
                    }

                    updatedProfiles.Add(profile);
                }
            }

            if (updatedProfiles.Any())
            {
                _repository.SetFields(updatedProfiles, p => p.FormatItems, p => p.MinFormatScore, p => p.CutoffFormatScore);
            }
        }

        private HashSet<int> GetCustomFormatIds()
        {
            using (var mapper = _database.OpenConnection())
            {
                return new HashSet<int>(mapper.Query<int>("SELECT Id FROM CustomFormats"));
            }
        }
    }

    public interface IQualityProfileFormatItemsCleanupRepository : IBasicRepository<QualityProfile>
    {
    }

    public class QualityProfileFormatItemsCleanupRepository : BasicRepository<QualityProfile>, IQualityProfileFormatItemsCleanupRepository
    {
        public QualityProfileFormatItemsCleanupRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
