using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.CustomFilters
{
    public interface ICustomFilterService
    {
        CustomFilter Add(CustomFilter customFilter);
        List<CustomFilter> All();
        void Delete(int id);
        CustomFilter Get(int id);
        CustomFilter Update(CustomFilter customFilter);
    }

    public class CustomFilterService : ICustomFilterService
    {
        private readonly ICustomFilterRepository _repo;

        public CustomFilterService(ICustomFilterRepository repo)
        {
            _repo = repo;
        }

        public CustomFilter Add(CustomFilter customFilter)
        {
            return _repo.Insert(customFilter);
        }

        public CustomFilter Update(CustomFilter customFilter)
        {
            return _repo.Update(customFilter);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public CustomFilter Get(int id)
        {
            return _repo.Get(id);
        }

        public List<CustomFilter> All()
        {
            return _repo.All().ToList();
        }
    }
}
