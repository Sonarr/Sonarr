using System;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class MigrationContext
    {
        public MigrationType MigrationType { get; private set; }
        public long? DesiredVersion { get; set; }

        public Action<NzbDroneMigrationBase> BeforeMigration { get; set; }

        public MigrationContext(MigrationType migrationType, long? desiredVersion = null)
        {
            MigrationType = migrationType;
            DesiredVersion = desiredVersion;
        }
    }
}