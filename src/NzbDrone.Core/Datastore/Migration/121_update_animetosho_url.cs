using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(121)]
    public class update_animetosho_url : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE Indexers SET Settings = Replace(Replace(Settings, '//animetosho.org', '//feed.animetosho.org'), '/feed/nabapi', '/nabapi') WHERE (Implementation = 'Newznab' OR Implementation = 'Torznab') AND Settings LIKE '%animetosho%';");
        }
    }

    public class IndexerDefinition121
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public JObject Settings { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public bool EnableRss { get; set; }
        public bool EnableAutomaticSearch { get; set; }
        public bool EnableInteractiveSearch { get; set; }
    }

    public class NewznabSettings121
    {
        public string BaseUrl { get; set; }

        public string ApiPath { get; set; }
    }
}
