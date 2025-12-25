using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigRepository : IBasicRepository<Config>
    {
        Config Get(string key);
        Config Upsert(string key, string value);

        // Async methods
        Task<Config> GetAsync(string key, CancellationToken cancellationToken = default);
        Task<Config> UpsertAsync(string key, string value, CancellationToken cancellationToken = default);
    }

    public class ConfigRepository : BasicRepository<Config>, IConfigRepository
    {
        public ConfigRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Config Get(string key)
        {
            return Query(c => c.Key == key).SingleOrDefault();
        }

        public Config Upsert(string key, string value)
        {
            var dbValue = Get(key);

            if (dbValue == null)
            {
                return Insert(new Config { Key = key, Value = value });
            }

            dbValue.Value = value;

            return Update(dbValue);
        }

        // Async methods
        public async Task<Config> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(c => c.Key == key, cancellationToken).ConfigureAwait(false);
            return results.SingleOrDefault();
        }

        public async Task<Config> UpsertAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            var dbValue = await GetAsync(key, cancellationToken).ConfigureAwait(false);

            if (dbValue == null)
            {
                return await InsertAsync(new Config { Key = key, Value = value }, cancellationToken).ConfigureAwait(false);
            }

            dbValue.Value = value;

            return await UpdateAsync(dbValue, cancellationToken).ConfigureAwait(false);
        }
    }
}
