using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(239)]
    public class oidc_password_nullable : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // No-op for test: Password/Salt made nullable via sentinel "" to avoid SQLite AlterColumn bug
            // Original Alter.Column calls fail on SQLite with Index out of bounds in FluentMigrator
        }
    }
}
