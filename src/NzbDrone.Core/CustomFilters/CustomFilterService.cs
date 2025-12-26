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
            return _repo.InsertAsync(customFilter).GetAwaiter().GetResult();
        }

        public CustomFilter Update(CustomFilter customFilter)
        {
            return _repo.UpdateAsync(customFilter).GetAwaiter().GetResult();
        }

        public void Delete(int id)
        {
            _repo.DeleteAsync(id).GetAwaiter().GetResult();
        }

        public CustomFilter Get(int id)
        {
            return _repo.GetAsync(id).GetAwaiter().GetResult();
        }

        public List<CustomFilter> All()
        {
            return _repo.AllAsync().GetAwaiter().GetResult().ToList();
        }
    }
}
