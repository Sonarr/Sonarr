using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Authentication
{
    public interface IUserRepository : IBasicRepository<User>
    {
        User FindUser(string username);
        User FindUser(Guid identifier);

        // Async methods
        Task<User> FindUserAsync(string username, CancellationToken cancellationToken = default);
        Task<User> FindUserAsync(Guid identifier, CancellationToken cancellationToken = default);
    }

    public class UserRepository : BasicRepository<User>, IUserRepository
    {
        public UserRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public User FindUser(string username)
        {
            return Query(x => x.Username == username).SingleOrDefault();
        }

        public User FindUser(Guid identifier)
        {
            return Query(x => x.Identifier == identifier).SingleOrDefault();
        }

        // Async methods
        public async Task<User> FindUserAsync(string username, CancellationToken cancellationToken = default)
        {
            var users = await QueryAsync(x => x.Username == username, cancellationToken).ConfigureAwait(false);
            return users.SingleOrDefault();
        }

        public async Task<User> FindUserAsync(Guid identifier, CancellationToken cancellationToken = default)
        {
            var users = await QueryAsync(x => x.Identifier == identifier, cancellationToken).ConfigureAwait(false);
            return users.SingleOrDefault();
        }
    }
}
