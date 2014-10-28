using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Core.Restrictions
{
    public interface IRestrictionService
    {
        List<Restriction> All();
        List<Restriction> AllForTag(Int32 tagId);
        List<Restriction> AllForTags(HashSet<Int32> tagIds);
        Restriction Get(Int32 id);
        void Delete(Int32 id);
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

        public List<Restriction> AllForTag(Int32 tagId)
        {
            return _repo.All().Where(r => r.Tags.Contains(tagId) || r.Tags.Empty()).ToList();
        }

        public List<Restriction> AllForTags(HashSet<Int32> tagIds)
        {
            return _repo.All().Where(r => r.Tags.Intersect(tagIds).Any() || r.Tags.Empty()).ToList();
        }

        public Restriction Get(Int32 id)
        {
            return _repo.Get(id);
        }

        public void Delete(Int32 id)
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
