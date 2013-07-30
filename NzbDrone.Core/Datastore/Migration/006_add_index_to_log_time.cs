using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(6)]
    public class add_index_to_log_time : NzbDroneMigrationBase
    {
        protected override void LogDbUpgrade()
        {
            Delete.Table("Logs");

            Create.TableForModel("Logs")
                  .WithColumn("Message").AsString()
                  .WithColumn("Time").AsDateTime().Indexed()
                  .WithColumn("Logger").AsString()
                  .WithColumn("Method").AsString().Nullable()
                  .WithColumn("Exception").AsString().Nullable()
                  .WithColumn("ExceptionType").AsString().Nullable()
                  .WithColumn("Level").AsString();
        }
    }
}
