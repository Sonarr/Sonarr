using System.Data;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(157)]
    public class email_multiple_addresses : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ChangeEmailAddressType);
        }

        private void ChangeEmailAddressType(IDbConnection conn, IDbTransaction tran)
        {
            using (var getEmailCmd = conn.CreateCommand())
            {
                getEmailCmd.Transaction = tran;
                getEmailCmd.CommandText = "SELECT Id, Settings FROM Notifications WHERE Implementation = 'Email'";

                using (var reader = getEmailCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var settings = Json.Deserialize<JObject>(reader.GetString(1));

                        // "To" was changed from string to array
                        settings["to"] = new JArray(settings["to"].ToObject<string>().Split(',').Select(v => v.Trim()).ToArray());

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE Notifications SET Settings = ? WHERE Id = ?";
                            updateCmd.AddParameter(settings.ToJson());
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
