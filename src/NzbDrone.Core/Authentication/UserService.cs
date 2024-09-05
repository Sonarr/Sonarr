using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Authentication
{
    public interface IUserService
    {
        User Add(string username, string password, UserRole role);
        User Update(User user);
        User Upsert(string username, string password);
        User FindUser();
        User FindUser(string username, string password);
        User FindUser(Guid identifier);
        User UpdateByModel(User updatedUser);
        public User FindUser(int id);

        void Delete(int id);

        public User FindUserFromApiKey(string apiKey);

        List<User> All();

        bool hasUsers();
    }

    public class UserService : IUserService, IHandle<ApplicationStartedEvent>
    {
        private const int ITERATIONS = 10000;
        private const int SALT_SIZE = 128 / 8;
        private const int NUMBER_OF_BYTES = 256 / 8;

        private readonly IUserRepository _repo;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;
        private readonly IEventAggregator _eventAggregator;

        public UserService(IUserRepository repo, IEventAggregator eventAggregator, IAppFolderInfo appFolderInfo, IDiskProvider diskProvider)
        {
            _repo = repo;
            _eventAggregator = eventAggregator;
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
        }

        public User Add(string username, string password, UserRole role)
        {
            var user = new User
            {
                Identifier = Guid.NewGuid(),
                Username = username.ToLowerInvariant(),
                Role = role,
                ApiKey = GenerateApiKey()
            };
            SetUserHashedPassword(user, password);

            return _repo.Insert(user);
        }

        public User Update(User user)
        {
            return _repo.Update(user);
        }

        // TODO: Do not allow to change username to something that already exists.
        public User UpdateByModel(User updatedUser)
        {
            var user = FindUser(updatedUser.Id);
            if (user == null)
            {
                return null;
            }

            user.Username = updatedUser.Username;
            user.Role = updatedUser.Role;
            if (!updatedUser.Password.IsNullOrWhiteSpace())
            {
                SetUserHashedPassword(user, updatedUser.Password);
            }

            return Update(user);
        }

        public User Upsert(string username, string password)
        {
            var user = FindUser();

            if (user == null)
            {
                return Add(username, password, UserRole.Admin);
            }

            if (user.Password != password)
            {
                SetUserHashedPassword(user, password);
            }

            user.Username = username.ToLowerInvariant();

            return Update(user);
        }

        public User FindUser()
        {
            return _repo.All().FirstOrDefault();
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

            if (user.Salt.IsNullOrWhiteSpace())
            {
                // If password matches stored SHA256 hash, update to salted hash and verify.
                if (user.Password == password.SHA256Hash())
                {
                    SetUserHashedPassword(user, password);

                    return Update(user);
                }

                return null;
            }

            if (VerifyHashedPassword(user, password))
            {
                return user;
            }

            return null;
        }

        public User FindUser(Guid identifier)
        {
            return _repo.FindUser(identifier);
        }

        public User FindUser(int id)
        {
            return _repo.Get(id);
        }

        public List<User> All()
        {
            return _repo.All().ToList();
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
            _eventAggregator.PublishEvent(new UsersUpdatedEvent());
        }

        public User FindUserFromApiKey(string apiKey)
        {
            return _repo.FindByApiKey(apiKey);
        }

        private User SetUserHashedPassword(User user, string password)
        {
            var salt = GenerateSalt();

            user.Iterations = ITERATIONS;
            user.Salt = Convert.ToBase64String(salt);
            user.Password = GetHashedPassword(password, salt, ITERATIONS);

            return user;
        }

        public bool hasUsers()
        {
            return _repo.All().Any();
        }

        private byte[] GenerateSalt()
        {
            var salt = new byte[SALT_SIZE];
            RandomNumberGenerator.Create().GetBytes(salt);

            return salt;
        }

        private string GenerateApiKey()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        private string GetHashedPassword(string password, byte[] salt, int iterations)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: iterations,
                numBytesRequested: NUMBER_OF_BYTES));
        }

        private bool VerifyHashedPassword(User user, string password)
        {
            var salt = Convert.FromBase64String(user.Salt);
            var hashedPassword = GetHashedPassword(password, salt, user.Iterations);

            return user.Password == hashedPassword;
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

            Add(username, password, UserRole.Admin);
        }
    }
}
