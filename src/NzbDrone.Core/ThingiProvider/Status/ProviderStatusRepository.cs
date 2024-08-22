using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider.Status
{
    public interface IProviderStatusRepository<TModel> : IBasicRepository<TModel>
        where TModel : ProviderStatusBase, new()
    {
        TModel FindByProviderId(int providerId);
        void DeleteByProviderId(int providerId);
    }

    public class ProviderStatusRepository<TModel> : BasicRepository<TModel>, IProviderStatusRepository<TModel>
        where TModel : ProviderStatusBase, new()
    {
        public ProviderStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public TModel FindByProviderId(int providerId)
        {
            return Query(c => c.ProviderId == providerId).SingleOrDefault();
        }

        public void DeleteByProviderId(int providerId)
        {
            Delete(c => c.ProviderId == providerId);
        }
    }
}
