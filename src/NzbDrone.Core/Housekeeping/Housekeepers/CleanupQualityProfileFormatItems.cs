using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupQualityProfileFormatItems : IHousekeepingTask
    {
        private readonly IQualityProfileFormatItemsCleanupRepository _repository;
        private readonly ICustomFormatRepository _customFormatRepository;

        public CleanupQualityProfileFormatItems(IQualityProfileFormatItemsCleanupRepository repository,
                                                ICustomFormatRepository customFormatRepository)
        {
            _repository = repository;
            _customFormatRepository = customFormatRepository;
        }

        public void Clean()
        {
            var test = _customFormatRepository.All();
            var customFormats = _customFormatRepository.All().ToDictionary(c => c.Id);
            var profiles = _repository.All();
            var updatedProfiles = new List<QualityProfile>();

            foreach (var profile in profiles)
            {
                var formatItems = new List<ProfileFormatItem>();

                // Make sure the profile doesn't include formats that have been removed
                profile.FormatItems.ForEach(p =>
                {
                    if (p.Format != null && customFormats.ContainsKey(p.Format.Id))
                    {
                        formatItems.Add(p);
                    }
                });

                // Make sure the profile includes all available formats
                foreach (var customFormat in customFormats)
                {
                    if (formatItems.None(f => f.Format.Id == customFormat.Key))
                    {
                        formatItems.Insert(0, new ProfileFormatItem
                        {
                            Format = customFormat.Value,
                            Score = 0
                        });
                    }
                }

                var previousIds = profile.FormatItems.Select(i => i.Format.Id).ToList();
                var ids = formatItems.Select(i => i.Format.Id).ToList();

                // Update the profile if any formats were added or removed
                if (ids.Except(previousIds).Any() || previousIds.Except(ids).Any())
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
