using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(156)]
    public class update_download_client_ssl_options : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(UpdateTransmissionSettings);
            Execute.WithConnection(UpdateNzbVortexSettings);
        }

        private void UpdateTransmissionSettings(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Id, Settings FROM DownloadClients WHERE Implementation = 'UTorrent'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var settingsJson = reader.GetString(1);

                        var settings = Json.Deserialize<Dictionary<string, object>>(settingsJson);

                            settings["useSSL"] = false;

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

        private void UpdateNzbVortexSettings(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Id, Settings FROM DownloadClients WHERE Implementation = 'NzbVortex'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var settingsJson = reader.GetString(1);

                        var settings = Json.Deserialize<Dictionary<string, object>>(settingsJson);

                        settings["useSSL"] = true;

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
