using FluentMigrator;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(213)]
    public class add_role_and_api_key_to_users : NzbDroneMigrationBase
    {
        private readonly IConfigFileProvider _configFileProvider;

        public add_role_and_api_key_to_users(IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
        }

        protected override void MainDbUpgrade()
        {
            var apiKey = _configFileProvider.ApiKey;
            Alter.Table("Users").AddColumn("Role").AsString().WithDefaultValue("Admin");
            Alter.Table("Users").AddColumn("ApiKey").AsString().WithDefaultValue(apiKey);
        }
    }
}
