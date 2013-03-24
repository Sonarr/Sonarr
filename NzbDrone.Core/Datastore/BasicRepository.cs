using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using ServiceStack.OrmLite;

namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        IEnumerable<TModel> All();
        int Count();
        bool Any(Expression<Func<TModel, bool>> predicate);
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
        void UpdateFields<TKey>(TModel model, Expression<Func<TModel, TKey>> onlyFields);
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        private readonly IDbConnection _database;

        public BasicRepository(IDbConnection database)
        {
            _database = database;
        }

        public IEnumerable<TModel> All()
        {
            return _database.Select<TModel>();
        }

        public int Count()
        {
            return (int)_database.Count<TModel>();
        }

        public bool Any(Expression<Func<TModel, bool>> predicate)
        {
            return _database.Exists<TModel>(predicate);
        }

        public TModel Get(int id)
        {
            try
            {
                return _database.GetById<TModel>(id);
            }
            catch (ArgumentNullException e)
            {
                throw new InvalidOperationException(e.Message);
            }

        }

        public TModel Single(Expression<Func<TModel, bool>> predicate)
        {
            return _database.Select(predicate).Single();
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
            return _database.Select(predicate).SingleOrDefault();
        }

        public List<TModel> Where(Expression<Func<TModel, bool>> predicate)
        {
            return _database.Select(predicate);
        }

        public TModel Insert(TModel model)
        {
            if (model.Id != 0)
            {
                throw new InvalidOperationException("Can't insert model with existing ID");
            }

            _database.Insert(model);
            model.Id = (int)_database.GetLastInsertId();
            return model;
        }

        public TModel Update(TModel model)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            _database.Update(model);
            return model;
        }


        public void Delete(TModel model)
        {
            _database.Delete(model);
        }

        public void InsertMany(IList<TModel> models)
        {
            _database.InsertAll(models);
        }

        public void UpdateMany(IList<TModel> models)
        {
            _database.UpdateAll(models);
        }

        public void DeleteMany(List<TModel> models)
        {
            _database.DeleteAll(models);
        }

        public TModel Upsert(TModel model)
        {
            if (model.Id == 0)
            {
                _database.Insert(model);
                model.Id = (int)_database.GetLastInsertId();
                return model;
            }
            _database.Update(model);
            return model;
        }

        public void Delete(int id)
        {
            _database.DeleteById<TModel>(id);
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            _database.DeleteByIds<TModel>(ids);
        }

        public void Purge()
        {
            _database.DeleteAll<TModel>();
        }

        public bool HasItems()
        {
            return Count() > 0;
        }

        public void UpdateFields<TKey>(TModel model, Expression<Func<TModel, TKey>> onlyFields)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Attempted to updated model without ID");
            }

            _database.UpdateOnly(model, onlyFields, m => m.Id == model.Id);
        }
    }
}
