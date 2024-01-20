using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(201)]
    public class email_encryption : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ChangeEncryption);
        }

        private void ChangeEncryption(IDbConnection conn, IDbTransaction tran)
        {
            var updated = new List<object>();
            using (var getEmailCmd = conn.CreateCommand())
            {
                getEmailCmd.Transaction = tran;
                getEmailCmd.CommandText = "SELECT \"Id\", \"Settings\" FROM \"Notifications\" WHERE \"Implementation\" = 'Email'";

                using (var reader = getEmailCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var settings = Json.Deserialize<JObject>(reader.GetString(1));

                        settings["useEncryption"] = settings["requireEncryption"].ToObject<bool>() ? 1 : 0;
                        settings["requireEncryption"] = null;

                        updated.Add(new
                        {
                            Settings = settings.ToJson(),
                            Id = id
                        });
                    }
                }
            }

            var updateSql = $"UPDATE \"Notifications\" SET \"Settings\" = @Settings WHERE \"Id\" = @Id";
            conn.Execute(updateSql, updated, transaction: tran);
        }
    }
}
