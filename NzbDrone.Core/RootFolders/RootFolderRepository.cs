using System.Collections.Generic;
using Eloquera.Client;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Repository;
using System.Linq;

namespace NzbDrone.Core.RootFolders
{

    public abstract class BaseModel
    {
        [ID]
        public int Id;
    }

    public interface IBasicRepository<TModel>
    {
        List<TModel> All();
        TModel Get(int rootFolderId);
        TModel Add(TModel rootFolder);
        void Delete(int rootFolderId);
    }


    public abstract class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : BaseModel, new()
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

    public interface IRootFolderRepository : IBasicRepository<RootDir>
    {

    }


    //This way we only need to implement none_custom methods for repos, like custom queries etc... rest is done automagically.
    public class RootFolderRepository : BasicRepository<RootDir>, IRootFolderRepository
    {
        public RootFolderRepository(EloqueraDb eloqueraDb)
            : base(eloqueraDb)
        {
        }
    }

}
