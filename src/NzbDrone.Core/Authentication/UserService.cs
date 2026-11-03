using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NzbDrone.Common.Extensions;

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
        User UpsertOidcUser(string subject, string username, string groups, SonarrRole role);
    }

    public class UserService : IUserService
    {
        private const int ITERATIONS = 220000;
        private const int SALT_SIZE = 128 / 8;
        private const int NUMBER_OF_BYTES = 256 / 8;

        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public User Add(string username, string password)
        {
            var user = new User
            {
                Identifier = Guid.NewGuid(),
                Username = username.ToLowerInvariant()
            };

            SetUserHashedPassword(user, password);

            return _repo.Insert(user);
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
                if (password == null)
                {
                    return null;
                }

                return Add(username, password);
            }

            if (password != null)
            {
                SetUserHashedPassword(user, password);
            }

            user.Username = username.ToLowerInvariant();

            return Update(user);
        }

        public User FindUser()
        {
            var local = (_repo.All() ?? Enumerable.Empty<User>())
                .Where(u => u.OidcSubject == null)
                .SingleOrDefault();

            if (local == null)
            {
                try
                {
                    local = _repo.FindLocalUser();
                }
                catch
                {
                    // Fall back to in-memory scan if the repository does not support FindLocalUser.
                }
            }

            return local;
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
                if (user.Password.IsNullOrWhiteSpace())
                {
                    return null;
                }

                // If password matches stored SHA256 hash, update to salted hash and verify.
                if (IsMatchingHash(user.Password, password.SHA256Hash()))
                {
                    SetUserHashedPassword(user, password);

                    return Update(user);
                }

                return null;
            }

            if (VerifyHashedPassword(user, password))
            {
                if (user.Iterations < ITERATIONS)
                {
                    return Update(SetUserHashedPassword(user, password));
                }

                return user;
            }

            return null;
        }

        public User FindUser(Guid identifier)
        {
            return _repo.FindUser(identifier);
        }

        public User UpsertOidcUser(string subject, string username, string groups, SonarrRole role)
        {
            var normalizedUsername = username == null ? null : username.ToLowerInvariant();

            // 1. Subject-first lookup.
            User user = null;

            if (!subject.IsNullOrWhiteSpace())
            {
                user = _repo.FindBySubject(subject);

                if (user == null)
                {
                    user = (_repo.All() ?? Enumerable.Empty<User>()).FirstOrDefault(u => u.OidcSubject == subject);
                }
            }

            if (user != null)
            {
                user.Username = normalizedUsername;
                user.OidcGroups = groups;
                user.Role = role;

                return _repo.Update(user);
            }

            // 3. Insert new OIDC user. Forms and OIDC users are kept separate;
            // no automatic linking by username to avoid account takeover via IdP-controlled preferred_username.
            var newUser = new User
            {
                Identifier = Guid.NewGuid(),
                Username = normalizedUsername,
                Password = "",
                Salt = "",
                Iterations = 0,
                OidcSubject = subject,
                OidcGroups = groups,
                Role = role
            };

            try
            {
                return _repo.Insert(newUser);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to insert OIDC user (username may already exist): " + ex.Message, ex);
            }
        }

        private User SetUserHashedPassword(User user, string password)
        {
            var salt = GenerateSalt();

            user.Iterations = ITERATIONS;
            user.Salt = Convert.ToBase64String(salt);
            user.Password = GetHashedPassword(password, salt, ITERATIONS);

            return user;
        }

        private byte[] GenerateSalt()
        {
            var salt = new byte[SALT_SIZE];
            RandomNumberGenerator.Create().GetBytes(salt);

            return salt;
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

            return IsMatchingHash(user.Password, hashedPassword);
        }

        private static bool IsMatchingHash(string storedHash, string hash)
        {
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(storedHash), Encoding.UTF8.GetBytes(hash));
        }
    }
}
