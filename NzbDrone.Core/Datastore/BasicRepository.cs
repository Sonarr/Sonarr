using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel>
    {
        List<TModel> All();
        TModel Get(long rootFolderId);
        TModel Add(TModel rootFolder);
        void Delete(long rootFolderId);
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : BaseRepositoryModel, new()
    {
        public BasicRepository(EloqueraDb eloqueraDb)
        {
            EloqueraDb = eloqueraDb;
        }

        protected EloqueraDb EloqueraDb { get; private set; }

        public List<TModel> All()
        {
            return EloqueraDb.AsQueryable<TModel>().ToList();
        }

        public TModel Get(long id)
        {
            return EloqueraDb.AsQueryable<TModel>().Single(c => c.Id == id);
        }

        public TModel Add(TModel model)
        {
            return EloqueraDb.Insert(model);
        }

        public void Delete(long id)
        {
            var itemToDelete = Get(id);
            EloqueraDb.Delete(itemToDelete);
        }
    }
}
