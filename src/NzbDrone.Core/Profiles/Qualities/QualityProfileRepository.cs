using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Qualities
{
    public interface IQualityProfileRepository : IBasicRepository<QualityProfile>
    {
        // Async
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    }

    public class QualityProfileRepository : BasicRepository<QualityProfile>, IQualityProfileRepository
    {
        private readonly ICustomFormatService _customFormatService;

        public QualityProfileRepository(IMainDatabase database,
                                        IEventAggregator eventAggregator,
                                        ICustomFormatService customFormatService)
            : base(database, eventAggregator)
        {
            _customFormatService = customFormatService;
        }

        protected override async Task<List<QualityProfile>> QueryAsync(SqlBuilder builder, CancellationToken cancellationToken = default)
        {
            var customFormats = _customFormatService.All();
            var cfs = customFormats.ToDictionary(c => c.Id);

            var profiles = await base.QueryAsync(builder, cancellationToken).ConfigureAwait(false);

            // Do the conversions from Id to full CustomFormat object here instead of in
            // CustomFormatIntConverter to remove need to for a static property containing
            // all the custom formats
            foreach (var profile in profiles)
            {
                var formatItems = new List<ProfileFormatItem>();

                foreach (var formatItem in profile.FormatItems)
                {
                    // Skip any format that has been removed, but the profile wasn't updated properly
                    if (cfs.ContainsKey(formatItem.Format.Id))
                    {
                        formatItem.Format = cfs[formatItem.Format.Id];

                        formatItems.Add(formatItem);
                    }
                }

                profile.FormatItems = formatItems;
            }

            return profiles;
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
            return results.Count == 1;
        }
    }
}
