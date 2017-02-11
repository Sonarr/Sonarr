using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Profiles.Delay
{
    public interface IDelayProfileService
    {
        DelayProfile Add(DelayProfile profile);
        DelayProfile Update(DelayProfile profile);
        void Delete(int id);
        List<DelayProfile> All();
        DelayProfile Get(int id);
        List<DelayProfile> AllForTag(int tagId);
        List<DelayProfile> AllForTags(HashSet<int> tagIds);
        DelayProfile BestForTags(HashSet<int> tagIds);
        List<DelayProfile> Reorder(int id, int? afterId);
    }

    public class DelayProfileService : IDelayProfileService
    {
        private readonly IDelayProfileRepository _repo;

        public DelayProfileService(IDelayProfileRepository repo)
        {
            _repo = repo;
        }

        public DelayProfile Add(DelayProfile profile)
        {
            profile.Order = _repo.Count();

            return _repo.Insert(profile);
        }

        public DelayProfile Update(DelayProfile profile)
        {
            return _repo.Update(profile);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);

            var all = All().OrderBy(d => d.Order).ToList();

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Id == 1) continue;

                all[i].Order = i + 1;
            }

            _repo.UpdateMany(all);
        }

        public List<DelayProfile> All()
        {
            return _repo.All().ToList();
        }

        public DelayProfile Get(int id)
        {
            return _repo.Get(id);
        }

        public List<DelayProfile> AllForTag(int tagId)
        {
            return All().Where(r => r.Tags.Contains(tagId))
                        .ToList();
        }

        public List<DelayProfile> AllForTags(HashSet<int> tagIds)
        {
            return All().Where(r => r.Tags.Intersect(tagIds).Any() || r.Tags.Empty()).ToList();
        }

        public DelayProfile BestForTags(HashSet<int> tagIds)
        {
            return _repo.All().Where(r => r.Tags.Intersect(tagIds).Any() || r.Tags.Empty())
                        .OrderBy(d => d.Order).First();
        }

        public List<DelayProfile> Reorder(int id, int? afterId)
        {
            var all = All().OrderBy(d => d.Order)
                           .ToList();

            var moving = all.SingleOrDefault(d => d.Id == id);
            var after = afterId.HasValue ? all.SingleOrDefault(d => d.Id == afterId) : null;

            if (moving == null)
            {
                // TODO: This should throw
                return all;
            }

            var afterOrder = GetAfterOrder(moving, after);
            var afterCount = afterOrder + 2;
            var movingOrder = moving.Order;

            foreach (var delayProfile in all)
            {
                if (delayProfile.Id == 1)
                {
                    continue;
                }

                if (delayProfile.Id == id)
                {
                    delayProfile.Order = afterOrder + 1;
                }

                else if (delayProfile.Id == after?.Id)
                {
                    delayProfile.Order = afterOrder;
                }

                else if (delayProfile.Order > afterOrder)
                {
                    delayProfile.Order = afterCount;
                    afterCount++;
                }
               
                else if (delayProfile.Order > movingOrder)
                {
                    delayProfile.Order--;
                }
            }

            _repo.UpdateMany(all);

            return All();
        }

        private int GetAfterOrder(DelayProfile moving, DelayProfile after)
        {
            if (after == null)
            {
                return 0;
            }

            if (moving.Order < after.Order)
            {
                return after.Order - 1;
            }

            return after.Order;
        }
    }
}
