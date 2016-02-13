using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using Newtonsoft.Json.Linq;
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

                            settings.Add("deviceIds", new[] { deviceId });
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

    public class Notification86
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OnGrab { get; set; }
        public int OnDownload { get; set; }
        public JObject Settings { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public int OnUpgrade { get; set; }
        public List<int> Tags { get; set; }
    }

    public class PushBulletSettings86
    {
        public string ApiKey { get; set; }
        public string[] DeviceIds { get; set; }
        public string ChannelTags { get; set; }
        public string SenderId { get; set; }
    }
}
