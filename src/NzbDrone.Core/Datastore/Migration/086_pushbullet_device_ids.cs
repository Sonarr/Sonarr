using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(86)]
    public class pushbullet_device_ids : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(UpdateTransmissionSettings);
        }

        private void UpdateTransmissionSettings(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Id, Settings FROM Notifications WHERE Implementation = 'PushBullet'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var settingsJson = reader.GetString(1);
                        var settings = Json.Deserialize<Dictionary<string, object>>(settingsJson);

                        if (settings.ContainsKey("deviceId"))
                        {
                            var deviceId = settings.GetValueOrDefault("deviceId", "") as string;

                            settings.Add("deviceIds", deviceId);
                            settings.Remove("deviceId");

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
}
