using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel>
    {
        List<TModel> All();
        TModel Get(int id);
        TModel Add(TModel model);
        TModel Update(TModel model);
        TModel Upsert(TModel model);
        void Delete(int id);
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        public BasicRepository(IObjectDatabase objectDatabase)
        {
            ObjectDatabase = objectDatabase;
        }

        public IObjectDatabase ObjectDatabase { get; private set; }

        protected IEnumerable<TModel> Queryable { get { return ObjectDatabase.AsQueryable<TModel>(); } }

        public List<TModel> All()
        {
            return Queryable.ToList();
        }

        public TModel Get(int id)
        {
            return Queryable.Single(c => c.OID == id);
        }

        public TModel Add(TModel model)
        {
            return ObjectDatabase.Insert(model);
        }

        public TModel Update(TModel model)
        {
            return ObjectDatabase.Update(model);
        }

        public TModel Upsert(TModel model)
        {
           if(model.OID == 0)
           {
               return ObjectDatabase.Insert(model);
           }
           return ObjectDatabase.Update(model);
        }

        public void Delete(int id)
        {
            var itemToDelete = Get(id);
            ObjectDatabase.Delete(itemToDelete);
        }
    }
}
