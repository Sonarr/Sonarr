using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(237)]
    public class download_client_submission_failures : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("DownloadClientStatus")
                 .AddColumn("SubmissionFailureCount").AsInt32().NotNullable().WithDefaultValue(0)
                 .AddColumn("MostRecentSubmissionFailure").AsDateTime().Nullable();
        }
    }
}
