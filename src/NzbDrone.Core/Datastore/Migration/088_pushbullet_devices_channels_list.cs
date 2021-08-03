using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(88)]
    public class pushbullet_devices_channels_list : NzbDroneMigrationBase
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

                        if (settings.ContainsKey("deviceIds"))
                        {
                            var deviceIdsString = settings.GetValueOrDefault("deviceIds", "") as string;

                            if (deviceIdsString.IsNotNullOrWhiteSpace())
                            {
                                var deviceIds = deviceIdsString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                                settings["deviceIds"] = deviceIds;
                            }
                        }

                        if (settings.ContainsKey("channelTags"))
                        {
                            var channelTagsString = settings.GetValueOrDefault("channelTags", "") as string;

                            if (channelTagsString.IsNotNullOrWhiteSpace())
                            {
                                var channelTags = channelTagsString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                                settings["channelTags"] = channelTags;
                            }
                        }

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

    public class PushBulletSettings88
    {
        public string ApiKey { get; set; }
        public string[] DeviceIds { get; set; }
        public string[] ChannelTags { get; set; }
        public string SenderId { get; set; }
    }
}
