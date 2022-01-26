using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(85)]
    public class expand_transmission_urlbase : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(UpdateTransmissionSettings);
        }

        private void UpdateTransmissionSettings(IDbConnection conn, IDbTransaction tran)
        {
            var updatedClients = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"Settings\" FROM \"DownloadClients\" WHERE \"Implementation\" = 'Transmission'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var settingsJson = reader.GetString(1);

                        var settings = Json.Deserialize<Dictionary<string, object>>(settingsJson);

                        var urlBase = settings.GetValueOrDefault("urlBase", "") as string;

                        if (urlBase.IsNullOrWhiteSpace())
                        {
                            settings["urlBase"] = "/transmission/";
                        }
                        else
                        {
                            settings["urlBase"] = string.Format("/{0}/transmission/", urlBase.Trim('/'));
                        }

                        updatedClients.Add(new
                        {
                            Settings = settings.ToJson(),
                            Id = id
                        });
                    }
                }
            }

            var updateClientsSql = "UPDATE \"DownloadClients\" SET \"Settings\" = @Settings WHERE \"Id\" = @Id";
            conn.Execute(updateClientsSql, updatedClients, transaction: tran);
        }
    }

    public class DelugeSettings85
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UrlBase { get; set; }
        public string Password { get; set; }
        public string TvCategory { get; set; }
        public int RecentTvPriority { get; set; }
        public int OlderTvPriority { get; set; }
        public bool UseSsl { get; set; }
    }
}
