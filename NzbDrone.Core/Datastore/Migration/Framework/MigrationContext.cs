namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class MigrationContext
    {
        public MigrationType MigrationType { get; set; }
        public ISQLiteAlter SQLiteAlter { get; set; }
    }
}