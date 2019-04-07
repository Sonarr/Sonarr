using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Authentication
{
    public interface IUserService
    {
        User Add(string username, string password);
        User Update(User user);
        User Upsert(string username, string password);
        User FindUser();
        User FindUser(string username, string password);
        User FindUser(Guid identifier);
    }

    public class UserService : IUserService, IHandle<ApplicationStartedEvent>
    {
        private readonly IUserRepository _repo;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public UserService(IUserRepository repo, IAppFolderInfo appFolderInfo, IDiskProvider diskProvider)
        {
            _repo = repo;
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
        }

        public User Add(string username, string password)
        {
            return _repo.Insert(new User
                                {
                                    Identifier = Guid.NewGuid(),
                                    Username = username.ToLowerInvariant(),
                                    Password = password.SHA256Hash()
                                });
        }

        public User Update(User user)
        {
            return _repo.Update(user);
        }

        public User Upsert(string username, string password)
        {
            var user = FindUser();

            if (user == null)
            {
                return Add(username, password);
            }

            if (user.Password != password)
            {
                user.Password = password.SHA256Hash();
            }

            user.Username = username.ToLowerInvariant();

            return Update(user);
        }

        public User FindUser()
        {
            return _repo.SingleOrDefault();
        }

        public User FindUser(string username, string password)
        {
            if (username.IsNullOrWhiteSpace() || password.IsNullOrWhiteSpace())
            {
                return null;
            }

            var user = _repo.FindUser(username.ToLowerInvariant());

            if (user == null)
            {
                return null;
            }

            if (user.Password == password.SHA256Hash())
            {
                return user;
            }

            return null;
        }

        public User FindUser(Guid identifier)
        {
            return _repo.FindUser(identifier);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (_repo.All().Any())
            {
                return;
            }

            var configFile = _appFolderInfo.GetConfigPath();

            if (!_diskProvider.FileExists(configFile))
            {
                return;
            }

            var xDoc = XDocument.Load(configFile);
            var config = xDoc.Descendants("Config").Single();
            var usernameElement = config.Descendants("Username").FirstOrDefault();
            var passwordElement = config.Descendants("Password").FirstOrDefault();

            if (usernameElement == null || passwordElement == null)
            {
                return;
            }

            var username = usernameElement.Value;
            var password = passwordElement.Value;

            Add(username, password);
        }
    }
}
