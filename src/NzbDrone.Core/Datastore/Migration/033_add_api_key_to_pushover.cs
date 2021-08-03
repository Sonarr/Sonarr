using System.Data;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(33)]
    public class add_api_key_to_pushover : NzbDroneMigrationBase
    {
        private const string API_KEY = "yz9b4U215iR4vrKFRfjNXP24NMNPKJ";

        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(UpdatePushoverSettings);
        }

        private void UpdatePushoverSettings(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand selectCommand = conn.CreateCommand())
            {
                selectCommand.Transaction = tran;
                selectCommand.CommandText = @"SELECT * FROM Notifications WHERE ConfigContract = 'PushoverSettings'";

                using (IDataReader reader = selectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var idIndex = reader.GetOrdinal("Id");
                        var settingsIndex = reader.GetOrdinal("Settings");

                        var id = reader.GetInt32(idIndex);
                        var settings = Json.Deserialize<PushoverSettingsForV33>(reader.GetString(settingsIndex));
                        settings.ApiKey = API_KEY;

                        //Set priority to high if its currently emergency
                        if (settings.Priority == 2)
                        {
                            settings.Priority = 1;
                        }

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            var text = string.Format("UPDATE Notifications " +
                                                     "SET Settings = '{0}'" +
                                                     "WHERE Id = {1}",
                                settings.ToJson(),
                                id);

                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = text;
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private class PushoverSettingsForV33
        {
            public string ApiKey { get; set; }
            public string UserKey { get; set; }
            public int Priority { get; set; }
        }
    }
}
