using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(81)]
    public class move_dot_prefix_to_transmission_category : NzbDroneMigrationBase
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
                cmd.CommandText = "SELECT Id, Settings FROM DownloadClients WHERE Implementation = 'Transmission'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var settingsJson = reader.GetString(1);

                        var settings = Json.Deserialize<Dictionary<string, object>>(settingsJson);

                        var tvCategory = settings.GetValueOrDefault("tvCategory") as string;
                        if (tvCategory.IsNotNullOrWhiteSpace())
                        {
                            settings["tvCategory"] = "." + tvCategory;

                            using (var updateCmd = conn.CreateCommand())
                            {
                                updateCmd.Transaction = tran;
                                updateCmd.CommandText = "UPDATE DownloadClients SET Settings = ? WHERE Id = ?";
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

    public class DownloadClientDefinition81
    {
        public int Id { get; set; }
        public bool Enable { get; set; }
        public string Name { get; set; }
        public string Implementation { get; set; }
        public JObject Settings { get; set; }
        public string ConfigContract { get; set; }
    }

    public class SabnzbdSettings81
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TvCategory { get; set; }
        public int RecentTvPriority { get; set; }
        public int OlderTvPriority { get; set; }
        public bool UseSsl { get; set; }
    }

    public class TransmissionSettings81
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UrlBase { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TvCategory { get; set; }
        public string TvDirectory { get; set; }
        public int RecentTvPriority { get; set; }
        public int OlderTvPriority { get; set; }
        public bool UseSsl { get; set; }
    }
}
