using System.Collections.Generic;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Datastore
{
    public abstract class CachedBasicRepository<TModel> : BasicRepository<TModel> where TModel : ModelBase, new()
    {
        private readonly ICacheManger _cacheManger;

        protected CachedBasicRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
            _cacheManger = new CacheManger();
        }

        protected ICached<T> GetCache<T>(string name)
        {
            return _cacheManger.GetCache<T>(GetType(), name);
        }

        protected override void OnModelChanged(IEnumerable<TModel> models)
        {
            PurgeCache();
        }

        protected override void OnModelDeleted(IEnumerable<TModel> models)
        {
            PurgeCache();
        }

        private void PurgeCache()
        {
            foreach (var model in _cacheManger.Caches)
            {
                model.Clear();
            }
        }
    }
}