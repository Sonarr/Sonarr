using FluentMigrator;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class MigrationOptions : IMigrationProcessorOptions
    {
        public bool PreviewOnly { get; set; }
        public int Timeout { get; set; }
        public string ProviderSwitches { get; private set; }
    }
}