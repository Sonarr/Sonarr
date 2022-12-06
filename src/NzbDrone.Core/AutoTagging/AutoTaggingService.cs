using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.AutoTagging
{
    public interface IAutoTaggingService
    {
        void Update(AutoTag autoTag);
        AutoTag Insert(AutoTag autoTag);
        List<AutoTag> All();
        AutoTag GetById(int id);
        void Delete(int id);
        List<AutoTag> AllForTag(int tagId);
        AutoTaggingChanges GetTagChanges(Series series);
    }

    public class AutoTaggingService : IAutoTaggingService
    {
        private readonly IAutoTaggingRepository _repository;
        private readonly RootFolderService _rootFolderService;
        private readonly ICached<Dictionary<int, AutoTag>> _cache;

        public AutoTaggingService(IAutoTaggingRepository repository,
                                  RootFolderService rootFolderService,
                                  ICacheManager cacheManager)
        {
            _repository = repository;
            _rootFolderService = rootFolderService;
            _cache = cacheManager.GetCache<Dictionary<int, AutoTag>>(typeof(AutoTag), "autoTags");
        }

        private Dictionary<int, AutoTag> AllDictionary()
        {
            return _cache.Get("all", () => _repository.All().ToDictionary(m => m.Id));
        }

        public List<AutoTag> All()
        {
            return AllDictionary().Values.ToList();
        }

        public AutoTag GetById(int id)
        {
            return AllDictionary()[id];
        }

        public void Update(AutoTag autoTag)
        {
            _repository.Update(autoTag);
            _cache.Clear();
        }

        public AutoTag Insert(AutoTag autoTag)
        {
            var result = _repository.Insert(autoTag);
            _cache.Clear();

            return result;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
            _cache.Clear();
        }

        public List<AutoTag> AllForTag(int tagId)
        {
            return All().Where(p => p.Tags.Contains(tagId))
                .ToList();
        }

        public AutoTaggingChanges GetTagChanges(Series series)
        {
            var autoTags = All();
            var changes = new AutoTaggingChanges();

            if (autoTags.Empty())
            {
                return changes;
            }

            // Set the root folder path on the series
            series.RootFolderPath = _rootFolderService.GetBestRootFolderPath(series.Path);

            foreach (var autoTag in autoTags)
            {
                var specificationMatches = autoTag.Specifications
                    .GroupBy(t => t.GetType())
                    .Select(g => new SpecificationMatchesGroup
                    {
                        Matches = g.ToDictionary(t => t, t => t.IsSatisfiedBy(series))
                    })
                    .ToList();

                var allMatch = specificationMatches.All(x => x.DidMatch);
                var tags = autoTag.Tags;

                if (allMatch)
                {
                    foreach (var tag in tags)
                    {
                        if (!series.Tags.Contains(tag))
                        {
                            changes.TagsToAdd.Add(tag);
                        }
                    }

                    continue;
                }

                if (autoTag.RemoveTagsAutomatically)
                {
                    foreach (var tag in tags)
                    {
                        changes.TagsToRemove.Add(tag);
                    }
                }
            }

            return changes;
        }
    }
}
