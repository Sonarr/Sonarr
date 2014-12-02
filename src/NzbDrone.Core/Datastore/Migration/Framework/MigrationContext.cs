using System;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class MigrationContext
    {
        public MigrationType MigrationType { get; private set; }

        public Action<NzbDroneMigrationBase> BeforeMigration { get; private set; }

        public MigrationContext(MigrationType migrationType, Action<NzbDroneMigrationBase> beforeAction)
        {
            MigrationType = migrationType;

            BeforeMigration = beforeAction;
        }
    }
}