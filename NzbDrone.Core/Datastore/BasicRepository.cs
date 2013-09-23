using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Marr.Data;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Common;
using NzbDrone.Core.Messaging.Events;


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
        PagingSpec<TModel> GetPaged(PagingSpec<TModel> pagingSpec);
    }


    public class BasicRepository<TModel> : IBasicRepository<TModel> where TModel : ModelBase, new()
    {
        private readonly IDatabase _database;
        private readonly IEventAggregator _eventAggregator;

        private IDataMapper DataMapper
        {
            get { return _database.GetDataMapper(); }
        }

        public BasicRepository(IDatabase database, IEventAggregator eventAggregator)
        {
            _database = database;
            _eventAggregator = eventAggregator;
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
            var model = DataMapper.Query<TModel>().SingleOrDefault(c => c.Id == id);

            if (model == null)
            {
                throw new ModelNotFoundException(typeof(TModel), id);
            }

            return model;
        }

        public IEnumerable<TModel> Get(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            var query = String.Format("Id IN ({0})", String.Join(",", idList));
            var result = Query.Where(query).ToList();

            if (result.Count != idList.Count())
            {
                throw new ApplicationException("Expected query to return {0} rows but returned {1}".Inject(idList.Count(), result.Count));
            }

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
                throw new InvalidOperationException("Can't insert model with existing ID " + model.Id);
            }

            DataMapper.Insert(model);

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


        public virtual PagingSpec<TModel> GetPaged(PagingSpec<TModel> pagingSpec)
        {
            var pagingQuery = Query.OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                                   .Skip(pagingSpec.PagingOffset())
                                   .Take(pagingSpec.PageSize);

            pagingSpec.Records = pagingQuery.ToList();

            //TODO: Use the same query for count and records
            pagingSpec.TotalRecords = Count();

            return pagingSpec;
        }

        public void DeleteAll()
        {
            DataMapper.Delete<TModel>(c => c.Id > 0);
        }

        protected void ModelCreated(TModel model)
        {
            PublishModelEvent(model, ModelAction.Created);
        }

        protected void ModelUpdated(TModel model)
        {
            PublishModelEvent(model, ModelAction.Updated);
        }

        protected void ModelDeleted(TModel model)
        {
            PublishModelEvent(model, ModelAction.Deleted);
        }

        private void PublishModelEvent(TModel model, ModelAction action)
        {
            if (PublishModelEvents)
            {
                _eventAggregator.PublishEvent(new ModelEvent<TModel>(model, action));
            }
        }

        protected virtual bool PublishModelEvents
        {
            get { return false; }
        }
    }
}
