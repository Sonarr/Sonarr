using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Profiles.Releases
{
    public interface IReleaseProfileService
    {
        List<ReleaseProfile> All();
        List<ReleaseProfile> AllForTag(int tagId);
        List<ReleaseProfile> AllForTags(HashSet<int> tagIds);
        ReleaseProfile Get(int id);
        void Delete(int id);
        ReleaseProfile Add(ReleaseProfile restriction);
        ReleaseProfile Update(ReleaseProfile restriction);
    }

    public class ReleaseProfileService : IReleaseProfileService
    {
        private readonly ReleaseProfilePreferredComparer _preferredComparer;
        private readonly IRestrictionRepository _repo;
        private readonly Logger _logger;

        public ReleaseProfileService(IRestrictionRepository repo, Logger logger)
        {
            _preferredComparer = new ReleaseProfilePreferredComparer();

            _repo = repo;
            _logger = logger;
        }

        public List<ReleaseProfile> All()
        {
            var all = _repo.All().ToList();
            all.ForEach(r => r.Preferred.Sort(_preferredComparer));

            return all;
        }

        public List<ReleaseProfile> AllForTag(int tagId)
        {
            return _repo.All().Where(r => r.Tags.Contains(tagId)).ToList();
        }

        public List<ReleaseProfile> AllForTags(HashSet<int> tagIds)
        {
            return _repo.All().Where(r => r.Tags.Intersect(tagIds).Any() || r.Tags.Empty()).ToList();
        }

        public ReleaseProfile Get(int id)
        {
            return _repo.Get(id);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public ReleaseProfile Add(ReleaseProfile restriction)
        {
            restriction.Preferred.Sort(_preferredComparer);

            return _repo.Insert(restriction);
        }

        public ReleaseProfile Update(ReleaseProfile restriction)
        {
            restriction.Preferred.Sort(_preferredComparer);

            return _repo.Update(restriction);
        }
    }
}
