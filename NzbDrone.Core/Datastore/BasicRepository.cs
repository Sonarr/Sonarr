using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel>
    {
        List<TModel> All();
        TModel Get(int rootFolderId);
        TModel Add(TModel rootFolder);
        void Delete(int rootFolderId);
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : BaseRepositoryModel, new()
    {
        public BasicRepository(IObjectDatabase objectDatabase)
        {
            ObjectDatabase = objectDatabase;
        }

        protected IObjectDatabase ObjectDatabase { get; private set; }

        public List<TModel> All()
        {
            return ObjectDatabase.AsQueryable<TModel>().ToList();
        }

        public TModel Get(int id)
        {
            return ObjectDatabase.AsQueryable<TModel>().Single(c => c.OID == id);
        }

        public TModel Add(TModel model)
        {
            return ObjectDatabase.Insert(model);
        }

        public void Delete(int id)
        {
            var itemToDelete = Get(id);
            ObjectDatabase.Delete(itemToDelete);
        }
    }
}
