using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(26)]
    public class add_config_contract_to_notifications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Notifications").Set(new { ConfigContract = "EmailSettings" }).Where(new { Implementation = "Email" });
            Update.Table("Notifications").Set(new { ConfigContract = "GrowlSettings" }).Where(new { Implementation = "Growl" });
            Update.Table("Notifications").Set(new { ConfigContract = "NotifyMyAndroidSettings" }).Where(new { Implementation = "NotifyMyAndroid" });
            Update.Table("Notifications").Set(new { ConfigContract = "PlexClientSettings" }).Where(new { Implementation = "PlexClient" });
            Update.Table("Notifications").Set(new { ConfigContract = "PlexServerSettings" }).Where(new { Implementation = "PlexServer" });
            Update.Table("Notifications").Set(new { ConfigContract = "ProwlSettings" }).Where(new { Implementation = "Prowl" });
            Update.Table("Notifications").Set(new { ConfigContract = "PushBulletSettings" }).Where(new { Implementation = "PushBullet" });
            Update.Table("Notifications").Set(new { ConfigContract = "PushoverSettings" }).Where(new { Implementation = "Pushover" });
            Update.Table("Notifications").Set(new { ConfigContract = "XbmcSettings" }).Where(new { Implementation = "Xbmc" });

            Delete.FromTable("Notifications").IsNull("ConfigContract");
        }
    }
}
