using System;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Authentication
{
    public interface IUserRepository : IBasicRepository<User>
    {
        User FindUser(string username);
        User FindUser(Guid identifier);
        User FindBySubject(string subject);
        User FindByUsername(string username);
        User FindLocalUser();
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

        public User FindBySubject(string subject)
        {
            return Query(x => x.OidcSubject == subject).SingleOrDefault();
        }

        public User FindByUsername(string username)
        {
            return Query(x => x.Username == username).SingleOrDefault();
        }

        public User FindLocalUser()
        {
            return Query(x => x.OidcSubject == null).SingleOrDefault();
        }
    }
}
