using NzbDrone.Core.Datastore.Migration.Framework;
using FluentMigrator;
using System.Data;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.UsenetBlackhole;
using Newtonsoft.Json;
using System;
using NzbDrone.Core.Download.Clients.Pneumatic;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(51)]
    public class rename_download_client_settings : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertFolderSettings);
        }

        private void ConvertFolderSettings(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand downloadClientsCmd = conn.CreateCommand())
            {
                downloadClientsCmd.Transaction = tran;
                downloadClientsCmd.CommandText = @"SELECT Value FROM Config WHERE Key = 'downloadedepisodesfolder'";
                var downloadedEpisodesFolder = downloadClientsCmd.ExecuteScalar() as String;

                downloadClientsCmd.Transaction = tran;
                downloadClientsCmd.CommandText = @"SELECT Id, Implementation, Settings, ConfigContract FROM DownloadClients WHERE ConfigContract = 'FolderSettings'";
                using (IDataReader downloadClientReader = downloadClientsCmd.ExecuteReader())
                {
                    while (downloadClientReader.Read())
                    {
                        var id = downloadClientReader.GetInt32(0);
                        var implementation = downloadClientReader.GetString(1);
                        var settings = downloadClientReader.GetString(2);
                        var configContract = downloadClientReader.GetString(3);

                        var settingsJson = JsonConvert.DeserializeObject(settings) as Newtonsoft.Json.Linq.JObject;

                        if (implementation == "Blackhole")
                        {
                            var newSettings = new
                            {
                                NzbFolder = settingsJson.Value<String>("folder"),
                                WatchFolder = downloadedEpisodesFolder
                            }.ToJson();

                            using (IDbCommand updateCmd = conn.CreateCommand())
                            {
                                updateCmd.Transaction = tran;
                                updateCmd.CommandText = "UPDATE DownloadClients SET Implementation = ?, Settings = ?, ConfigContract = ? WHERE Id = ?";
                                updateCmd.AddParameter("UsenetBlackhole");
                                updateCmd.AddParameter(newSettings);
                                updateCmd.AddParameter("UsenetBlackholeSettings");
                                updateCmd.AddParameter(id);

                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else if (implementation == "Pneumatic")
                        {
                            var newSettings = new
                            {
                                NzbFolder = settingsJson.Value<String>("folder")
                            }.ToJson();

                            using (IDbCommand updateCmd = conn.CreateCommand())
                            {
                                updateCmd.Transaction = tran;
                                updateCmd.CommandText = "UPDATE DownloadClients SET Settings = ?, ConfigContract = ? WHERE Id = ?";
                                updateCmd.AddParameter(newSettings);
                                updateCmd.AddParameter("PneumaticSettings");
                                updateCmd.AddParameter(id);

                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }
}
    }
}
