using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel>
    {
        List<TModel> All();
        TModel Get(int rootFolderId);
        TModel Add(TModel rootFolder);
        void Delete(int rootFolderId);
    }

    public abstract class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : BaseRepositoryModel, new()
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

        public TModel Get(int rootFolderId)
        {
            return EloqueraDb.AsQueryable<TModel>().Single(c => c.Id == rootFolderId);
        }

        public TModel Add(TModel rootFolder)
        {
            return EloqueraDb.Insert(rootFolder);
        }

        public void Delete(int rootFolderId)
        {
            var itemToDelete = Get(rootFolderId);
            EloqueraDb.Delete(itemToDelete);
        }
    }
}
