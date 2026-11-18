using System;
using Dapper;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOldOidcLogouts : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOldOidcLogouts(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            // Any session older than the OIDC cookie lifetime has already expired or been rejected
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""OidcLogouts"" WHERE ""Timestamp"" < @Cutoff", new { Cutoff = DateTime.UtcNow.Subtract(AuthenticationConfigurationExtensions.OidcSessionLifetime) });
        }
    }
}
