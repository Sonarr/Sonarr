using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Restrictions
{
    public interface IRestrictionService
    {
        List<Restriction> All();
        List<Restriction> AllForTag(int tagId);
        List<Restriction> AllForTags(HashSet<int> tagIds);
        Restriction Get(int id);
        void Delete(int id);
        Restriction Add(Restriction restriction);
        Restriction Update(Restriction restriction);
    }

    public class RestrictionService : IRestrictionService
    {
        private readonly IRestrictionRepository _repo;
        private readonly Logger _logger;

        public RestrictionService(IRestrictionRepository repo, Logger logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public List<Restriction> All()
        {
            return _repo.All().ToList();
        }

        public List<Restriction> AllForTag(int tagId)
        {
            return _repo.All().Where(r => r.Tags.Contains(tagId)).ToList();
        }

        public List<Restriction> AllForTags(HashSet<int> tagIds)
        {
            return _repo.All().Where(r => r.Tags.Intersect(tagIds).Any() || r.Tags.Empty()).ToList();
        }

        public Restriction Get(int id)
        {
            return _repo.Get(id);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public Restriction Add(Restriction restriction)
        {
            return _repo.Insert(restriction);
        }

        public Restriction Update(Restriction restriction)
        {
            return _repo.Update(restriction);
        }
    }
}
