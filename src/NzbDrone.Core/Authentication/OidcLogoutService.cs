using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Authentication
{
    public interface IOidcLogoutService
    {
        void LogoutSession(string sessionId);
        void LogoutSubject(string subject);
        bool IsLoggedOut(string sessionId, string subject, DateTime? issuedUtc);
    }

    public class OidcLogoutService : IOidcLogoutService
    {
        private readonly IOidcLogoutRepository _repository;

        // Checked on every authenticated request, so keep the logouts in memory instead of querying the database
        private readonly ICached<List<OidcLogout>> _cache;

        public OidcLogoutService(IOidcLogoutRepository repository, ICacheManager cacheManager)
        {
            _repository = repository;
            _cache = cacheManager.GetCache<List<OidcLogout>>(GetType());
        }

        public void LogoutSession(string sessionId)
        {
            _repository.Insert(new OidcLogout { SessionId = sessionId, Timestamp = DateTime.UtcNow });
            _cache.Clear();
        }

        public void LogoutSubject(string subject)
        {
            _repository.Insert(new OidcLogout { Subject = subject, Timestamp = DateTime.UtcNow });
            _cache.Clear();
        }

        public bool IsLoggedOut(string sessionId, string subject, DateTime? issuedUtc)
        {
            var logouts = _cache.Get("all", () => _repository.All().ToList());

            if (sessionId.IsNotNullOrWhiteSpace() && logouts.Any(l => l.SessionId == sessionId))
            {
                return true;
            }

            if (subject.IsNullOrWhiteSpace())
            {
                return false;
            }

            return logouts.Any(l => l.Subject == subject && (!issuedUtc.HasValue || issuedUtc.Value <= l.Timestamp));
        }
    }
}
