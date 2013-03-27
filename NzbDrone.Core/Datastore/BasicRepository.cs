using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Marr.Data;
using Marr.Data.QGen;


namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        IEnumerable<TModel> All();
        int Count();
        TModel Get(int id);
        TModel SingleOrDefault();
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
        void SetFields(TModel model, params Expression<Func<TModel, object>>[] properties);
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        private readonly IDataMapper _dataMapper;

        public BasicRepository(IDatabase database)
        {
            _dataMapper = database.DataMapper;
        }

        protected QueryBuilder<TModel> Query
        {
            get { return _dataMapper.Query<TModel>(); }
        }

        protected void Delete(Expression<Func<TModel, bool>> filter)
        {
            _dataMapper.Delete(filter);
        }

        public IEnumerable<TModel> All()
        {
            return _dataMapper.Query<TModel>().ToList();
        }

        public int Count()
        {
            return _dataMapper.Query<TModel>().Count();
        }

        public TModel Get(int id)
        {
            return _dataMapper.Query<TModel>().Single(c => c.Id == id);
        }

        public TModel SingleOrDefault()
        {
            return All().Single();
        }

        public TModel Insert(TModel model)
        {
            if (model.Id != 0)
            {
                throw new InvalidOperationException("Can't insert model with existing ID");
            }

            var id = _dataMapper.Insert(model);
            return model;
        }

        public TModel Update(TModel model)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            _dataMapper.Update(model, c => c.Id == model.Id);
            return model;
        }


        public void Delete(TModel model)
        {
            _dataMapper.Delete<TModel>(c => c.Id == model.Id);
        }

        public void InsertMany(IList<TModel> models)
        {
            foreach (var model in models)
            {
                Insert(model);
            }
        }

        public void UpdateMany(IList<TModel> models)
        {
            foreach (var model in models)
            {
                Update(model);
            }
        }

        public void DeleteMany(List<TModel> models)
        {
            models.ForEach(Delete);
        }

        public TModel Upsert(TModel model)
        {
            if (model.Id == 0)
            {
                Insert(model);
                return model;
            }
            Update(model);
            return model;
        }

        public void Delete(int id)
        {
            _dataMapper.Delete<TModel>(c => c.Id == id);
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            ids.ToList().ForEach(Delete);
        }

        public void Purge()
        {
            _dataMapper.Delete<TModel>(c => c.Id > -1);
        }

        public bool HasItems()
        {
            return Count() > 0;
        }

        public void SetFields(TModel model, params Expression<Func<TModel, object>>[] properties)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Attempted to updated model without ID");
            }

            _dataMapper.Update<TModel>()
                .Where(c => c.Id == model.Id)
                .ColumnsIncluding(properties)
                .Entity(model)
                .Execute();
        }

    }
}
