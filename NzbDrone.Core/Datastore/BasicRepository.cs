using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Marr.Data;
using Marr.Data.QGen;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore.Events;


namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        IEnumerable<TModel> All();
        int Count();
        TModel Get(int id);
        IEnumerable<TModel> Get(IEnumerable<int> ids);
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
        TModel Single();
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        private readonly IDatabase _database;
        private readonly IMessageAggregator _messageAggregator;

        //TODO: add assertion to make sure model properly mapped 


        private IDataMapper DataMapper
        {
            get { return _database.DataMapper; }
        }

        public BasicRepository(IDatabase database, IMessageAggregator messageAggregator)
        {
            _database = database;
            _messageAggregator = messageAggregator;
        }

        protected QueryBuilder<TModel> Query
        {
            get { return DataMapper.Query<TModel>(); }
        }

        protected void Delete(Expression<Func<TModel, bool>> filter)
        {
            DataMapper.Delete(filter);
        }

        public IEnumerable<TModel> All()
        {
            return DataMapper.Query<TModel>().ToList();
        }

        public int Count()
        {
            return DataMapper.Query<TModel>().GetRowCount();
        }

        public TModel Get(int id)
        {
            return DataMapper.Query<TModel>().Single(c => c.Id == id);
        }

        public IEnumerable<TModel> Get(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            var result = Query.Where(String.Format("Id IN ({0})", String.Join(",", idList))).ToList();
            var resultCount = result.Count;

            if (resultCount != idList.Count || result.Select(r => r.Id).Distinct().Count() != resultCount)
                throw new InvalidOperationException("Unexpected result from query");

            return result;
        }

        public TModel SingleOrDefault()
        {
            return All().SingleOrDefault();
        }

        public TModel Single()
        {
            return All().Single();
        }

        public TModel Insert(TModel model)
        {
            if (model.Id != 0)
            {
                throw new InvalidOperationException("Can't insert model with existing ID");
            }

            DataMapper.Insert(model);
            PublishModelEvent(model, RepositoryAction.Created);

            return model;
        }

        public TModel Update(TModel model)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            DataMapper.Update(model, c => c.Id == model.Id);
            return model;
        }

        public void Delete(TModel model)
        {
            DataMapper.Delete<TModel>(c => c.Id == model.Id);
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
            DataMapper.Delete<TModel>(c => c.Id == id);
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            ids.ToList().ForEach(Delete);
        }

        public void Purge()
        {
            DataMapper.Delete<TModel>(c => c.Id > -1);
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

            DataMapper.Update<TModel>()
                .Where(c => c.Id == model.Id)
                .ColumnsIncluding(properties)
                .Entity(model)
                .Execute();
        }


        protected virtual void PublishModelEvent(TModel model, RepositoryAction action)
        {
            _messageAggregator.PublishEvent(new ModelEvent<TModel>(model, action));
        }

    }
}
