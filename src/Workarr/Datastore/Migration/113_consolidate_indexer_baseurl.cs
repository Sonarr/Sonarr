using System.Data;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using Workarr.Datastore.Migrations.Framework;
using Workarr.Extensions;
using Workarr.Serializer.Newtonsoft.Json;

namespace Workarr.Datastore.Migrations
{
    [Migration(113)]
    public class consolidate_indexer_baseurl : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(RenameUrlToBaseUrl);
        }

        private void RenameUrlToBaseUrl(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"Settings\" FROM \"Indexers\" WHERE \"ConfigContract\" IN ('NewznabSettings', 'TorznabSettings', 'IPTorrentsSettings', 'OmgwtfnzbsSettings')";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var settings = reader.GetString(1);

                        if (settings.IsNotNullOrWhiteSpace())
                        {
                            var jsonObject = Json.Deserialize<JObject>(settings);

                            if (jsonObject.Property("url") != null)
                            {
                                jsonObject.AddFirst(new JProperty("baseUrl", jsonObject["url"]));
                                jsonObject.Remove("url");
                                settings = jsonObject.ToJson();

                                using (var updateCmd = conn.CreateCommand())
                                {
                                    updateCmd.Transaction = tran;
                                    updateCmd.CommandText = "UPDATE \"Indexers\" SET \"Settings\" = ? WHERE \"Id\" = ?";
                                    updateCmd.AddParameter(settings);
                                    updateCmd.AddParameter(id);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
