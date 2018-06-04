using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(101)]
    public class add_ultrahd_quality_in_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater71(conn, tran);
            updater.AppendQuality(16); // HDTV2160p
            updater.AppendQuality(18); // WEBDL2160p
            updater.AppendQuality(19); // Bluray2160p
            updater.Commit();

            // WEBRip migrations.
            //updater.SplitQualityAppend(1, 11);   // HDTV480p    after  SDTV
            //updater.SplitQualityPrepend(8, 12);  // WEBRip480p  before WEBDL480p
            //updater.SplitQualityAppend(2, 13);   // Bluray480p  after  DVD
            //updater.SplitQualityPrepend(5, 14);  // WEBRip720p  before WEBDL720p
            //updater.SplitQualityPrepend(3, 15);  // WEBRip1080p before WEBDL1080p
            //updater.SplitQualityPrepend(18, 17); // WEBRip2160p before WEBDL2160p
        }
    }
}
