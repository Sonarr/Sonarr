using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.AutoTagging.Specifications;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Tags
{
    public interface ITagService
    {
        Tag GetTag(int tagId);
        Tag GetTag(string tag);
        List<Tag> GetTags(IEnumerable<int> ids);
        TagDetails Details(int tagId);
        List<TagDetails> Details();
        List<Tag> All();
        Tag Add(Tag tag);
        Tag Update(Tag tag);
        void Delete(int tagId);
    }

    public class TagService : ITagService
    {
        private readonly ITagRepository _repo;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDelayProfileService _delayProfileService;
        private readonly IImportListFactory _importListFactory;
        private readonly INotificationFactory _notificationFactory;
        private readonly IReleaseProfileService _releaseProfileService;
        private readonly ISeriesService _seriesService;
        private readonly IIndexerFactory _indexerService;
        private readonly IAutoTaggingService _autoTaggingService;
        private readonly IDownloadClientFactory _downloadClientFactory;

        public TagService(ITagRepository repo,
                          IEventAggregator eventAggregator,
                          IDelayProfileService delayProfileService,
                          IImportListFactory importListFactory,
                          INotificationFactory notificationFactory,
                          IReleaseProfileService releaseProfileService,
                          ISeriesService seriesService,
                          IIndexerFactory indexerService,
                          IAutoTaggingService autoTaggingService,
                          IDownloadClientFactory downloadClientFactory)
        {
            _repo = repo;
            _eventAggregator = eventAggregator;
            _delayProfileService = delayProfileService;
            _importListFactory = importListFactory;
            _notificationFactory = notificationFactory;
            _releaseProfileService = releaseProfileService;
            _seriesService = seriesService;
            _indexerService = indexerService;
            _autoTaggingService = autoTaggingService;
            _downloadClientFactory = downloadClientFactory;
        }

        public Tag GetTag(int tagId)
        {
            return _repo.Get(tagId);
        }

        public Tag GetTag(string tag)
        {
            if (tag.All(char.IsDigit))
            {
                return _repo.Get(int.Parse(tag));
            }
            else
            {
                return _repo.GetByLabel(tag);
            }
        }

        public List<Tag> GetTags(IEnumerable<int> ids)
        {
            return _repo.Get(ids).ToList();
        }

        public TagDetails Details(int tagId)
        {
            var tag = GetTag(tagId);
            var delayProfiles = _delayProfileService.AllForTag(tagId);
            var importLists = _importListFactory.AllForTag(tagId);
            var notifications = _notificationFactory.AllForTag(tagId);
            var releaseProfiles = _releaseProfileService.AllForTag(tagId);
            var excludedReleaseProfiles = _releaseProfileService.AllExcludedForTag(tagId);
            var series = _seriesService.AllForTag(tagId);
            var indexers = _indexerService.AllForTag(tagId);
            var autoTags = _autoTaggingService.AllForTag(tagId);
            var downloadClients = _downloadClientFactory.AllForTag(tagId);

            return new TagDetails
            {
                Id = tagId,
                Label = tag.Label,
                DelayProfileIds = delayProfiles.Select(c => c.Id).ToList(),
                ImportListIds = importLists.Select(c => c.Id).ToList(),
                NotificationIds = notifications.Select(c => c.Id).ToList(),
                RestrictionIds = releaseProfiles.Select(c => c.Id).ToList(),
                ExcludedReleaseProfileIds = excludedReleaseProfiles.Select(c => c.Id).ToList(),
                SeriesIds = series.Select(c => c.Id).ToList(),
                IndexerIds = indexers.Select(c => c.Id).ToList(),
                AutoTagIds = autoTags.Select(c => c.Id).ToList(),
                DownloadClientIds = downloadClients.Select(c => c.Id).ToList()
            };
        }

        public List<TagDetails> Details()
        {
            var tags = All();
            var delayProfiles = _delayProfileService.All();
            var importLists = _importListFactory.All();
            var notifications = _notificationFactory.All();
            var releaseProfiles = _releaseProfileService.All();
            var excludedReleaseProfiles = _releaseProfileService.All();
            var series = _seriesService.GetAllSeriesTags();
            var indexers = _indexerService.All();
            var autoTags = _autoTaggingService.All();
            var downloadClients = _downloadClientFactory.All();

            var details = new List<TagDetails>();

            foreach (var tag in tags)
            {
                details.Add(new TagDetails
                    {
                        Id = tag.Id,
                        Label = tag.Label,
                        DelayProfileIds = delayProfiles.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                        ImportListIds = importLists.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                        NotificationIds = notifications.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                        RestrictionIds = releaseProfiles.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                        ExcludedReleaseProfileIds = excludedReleaseProfiles.Where(c => c.ExcludedTags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                        SeriesIds = series.Where(c => c.Value.Contains(tag.Id)).Select(c => c.Key).ToList(),
                        IndexerIds = indexers.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                        AutoTagIds = GetAutoTagIds(tag, autoTags),
                        DownloadClientIds = downloadClients.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                    });
            }

            return details;
        }

        public List<Tag> All()
        {
            return _repo.All().OrderBy(t => t.Label).ToList();
        }

        public Tag Add(Tag tag)
        {
            var existingTag = _repo.FindByLabel(tag.Label);

            if (existingTag != null)
            {
                return existingTag;
            }

            tag.Label = tag.Label.ToLowerInvariant();

            _repo.Insert(tag);
            _eventAggregator.PublishEvent(new TagsUpdatedEvent());

            return tag;
        }

        public Tag Update(Tag tag)
        {
            tag.Label = tag.Label.ToLowerInvariant();

            _repo.Update(tag);
            _eventAggregator.PublishEvent(new TagsUpdatedEvent());

            return tag;
        }

        public void Delete(int tagId)
        {
            var details = Details(tagId);
            if (details.InUse)
            {
                throw new ModelConflictException(typeof(Tag), tagId, $"'{details.Label}' cannot be deleted since it's still in use");
            }

            _repo.Delete(tagId);
            _eventAggregator.PublishEvent(new TagsUpdatedEvent());
        }

        private List<int> GetAutoTagIds(Tag tag, List<AutoTag> autoTags)
        {
            var autoTagIds = autoTags.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList();

            foreach (var autoTag in autoTags)
            {
                foreach (var specification in autoTag.Specifications)
                {
                    if (specification is TagSpecification tagSpecification && tagSpecification.Value == tag.Id)
                    {
                        autoTagIds.Add(autoTag.Id);
                    }
                }
            }

            return autoTagIds.Distinct().ToList();
        }
    }
}
