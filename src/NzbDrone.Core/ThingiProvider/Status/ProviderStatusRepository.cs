using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider.Status
{
    public interface IProviderStatusRepository<TModel> : IBasicRepository<TModel>
        where TModel : ProviderStatusBase, new()
    {
        TModel FindByProviderId(int providerId);
        void DeleteByProviderId(int providerId);

        // Async
        Task<TModel> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken = default);
        Task DeleteByProviderIdAsync(int providerId, CancellationToken cancellationToken = default);
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

        // Async

        public async Task<TModel> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(c => c.ProviderId == providerId, cancellationToken).ConfigureAwait(false);
            return results.SingleOrDefault();
        }

        public async Task DeleteByProviderIdAsync(int providerId, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(c => c.ProviderId == providerId, cancellationToken).ConfigureAwait(false);
        }
    }
}
