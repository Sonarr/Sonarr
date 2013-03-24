using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using NzbDrone.Core.Tv;
using ServiceStack.OrmLite;

namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        IEnumerable<TModel> All();
        int Count();
        TModel Get(int id);
        TModel Single(Expression<Func<TModel, bool>> predicate);
        TModel SingleOrDefault();
        TModel SingleOrDefault(Expression<Func<TModel, bool>> predicate);
        List<TModel> Where(Expression<Func<TModel, bool>> predicate);
        TModel Insert(TModel model);
        TModel Update(TModel model);
        TModel Upsert(TModel model);
        void Delete(int id);
        void Delete(TModel model);
        void InsertMany(IList<TModel> model);
        void UpdateMany(IList<TModel> model);
        void DeleteMany(List<TModel> model);
        void Purge();
        bool HasItems();
        void DeleteMany(IEnumerable<int> ids);
        void UpdateOnly<TKey>(TModel model, Expression<Func<TModel, TKey>> onlyFields);
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        public BasicRepository(IDbConnection database)
        {
            Database = database;
        }

        public IDbConnection Database { get; private set; }

        public IEnumerable<TModel> All()
        {
            return Database.Select<TModel>();
        }

        public int Count()
        {
            return (int)Database.Count<TModel>();
        }

        public TModel Get(int id)
        {
            return Database.GetById<TModel>(id);
        }

        public TModel Single(Expression<Func<TModel, bool>> predicate)
        {
            return Database.Select(predicate).Single();
        }

        public TModel SingleOrDefault()
        {
            return All().Single();
        }

        public TModel Single()
        {
            throw new System.NotImplementedException();
        }

        public TModel SingleOrDefault(Expression<Func<TModel, bool>> predicate)
        {
            return Database.Select(predicate).SingleOrDefault();
        }

        public List<TModel> Where(Expression<Func<TModel, bool>> predicate)
        {
            return Database.Select(predicate);
        }

        public TModel Insert(TModel model)
        {
            Database.Insert(model);
            model.Id = (int)Database.GetLastInsertId();
            return model;
        }

        public TModel Update(TModel model)
        {
            Database.Update(model);
            return model;
        }


        public void Delete(TModel model)
        {
            Database.Delete(model);
        }

        public void InsertMany(IList<TModel> models)
        {
            Database.InsertAll(models);
        }

        public void UpdateMany(IList<TModel> models)
        {
            Database.UpdateAll(models);
        }

        public void DeleteMany(List<TModel> models)
        {
            Database.DeleteAll(models);
        }

        public TModel Upsert(TModel model)
        {
            if (model.Id == 0)
            {
                Database.Insert(model);
                model.Id = (int)Database.GetLastInsertId();
                return model;
            }
            Database.Update(model);
            return model;
        }

        public void Delete(int id)
        {
            Database.DeleteById<TModel>(id);
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            Database.DeleteByIds<TModel>(ids);
        }

        public void Purge()
        {
            Database.DeleteAll<TModel>();
        }

        public bool HasItems()
        {
            return Count() > 0;
        }

        public void UpdateOnly<TKey>(TModel model, Expression<Func<TModel, TKey>> onlyFields)
        {
            Database.UpdateOnly(model, onlyFields);
        }
    }
}
