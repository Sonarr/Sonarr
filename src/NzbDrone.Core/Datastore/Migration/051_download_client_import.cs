using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(51)]
    public class download_client_import : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(EnableCompletedDownloadHandlingForNewUsers);

            Execute.WithConnection(ConvertFolderSettings);

            Execute.WithConnection(AssociateImportedHistoryItems);
        }

        private void EnableCompletedDownloadHandlingForNewUsers(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = @"SELECT Value FROM Config WHERE Key = 'downloadedepisodesfolder'";

                var result = cmd.ExecuteScalar();

                if (result == null)
                {
                    cmd.CommandText = @"INSERT INTO Config (Key, Value) VALUES ('enablecompleteddownloadhandling', 'True')";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ConvertFolderSettings(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand downloadClientsCmd = conn.CreateCommand())
            {
                downloadClientsCmd.Transaction = tran;
                downloadClientsCmd.CommandText = @"SELECT Value FROM Config WHERE Key = 'downloadedepisodesfolder'";
                var downloadedEpisodesFolder = downloadClientsCmd.ExecuteScalar() as string;

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
                                NzbFolder = settingsJson.Value<string>("folder"),
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
                                NzbFolder = settingsJson.Value<string>("folder")
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
                            using (IDbCommand updateCmd = conn.CreateCommand())
                            {
                                updateCmd.Transaction = tran;
                                updateCmd.CommandText = "DELETE FROM DownloadClients WHERE Id = ?";
                                updateCmd.AddParameter(id);

                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        private sealed class MigrationHistoryItem
        {
            public int Id { get; set; }
            public int EpisodeId { get; set; }
            public int SeriesId { get; set; }
            public string SourceTitle { get; set; }
            public DateTime Date { get; set; }
            public Dictionary<string, string> Data { get; set; }
            public MigrationHistoryEventType EventType { get; set; }
        }

        private enum MigrationHistoryEventType
        {
            Unknown = 0,
            Grabbed = 1,
            SeriesFolderImported = 2,
            DownloadFolderImported = 3,
            DownloadFailed = 4
        }

        private void AssociateImportedHistoryItems(IDbConnection conn, IDbTransaction tran)
        {
            var historyItems = new List<MigrationHistoryItem>();

            using (IDbCommand historyCmd = conn.CreateCommand())
            {
                historyCmd.Transaction = tran;
                historyCmd.CommandText = @"SELECT Id, EpisodeId, SeriesId, SourceTitle, Date, Data, EventType FROM History WHERE EventType NOT NULL";
                using (IDataReader historyRead = historyCmd.ExecuteReader())
                {
                    while (historyRead.Read())
                    {
                        historyItems.Add(new MigrationHistoryItem
                            {
                                Id = historyRead.GetInt32(0),
                                EpisodeId = historyRead.GetInt32(1),
                                SeriesId = historyRead.GetInt32(2),
                                SourceTitle = historyRead.GetString(3),
                                Date = historyRead.GetDateTime(4),
                                Data = Json.Deserialize<Dictionary<string, string>>(historyRead.GetString(5)),
                                EventType = (MigrationHistoryEventType)historyRead.GetInt32(6)
                            });
                    }
                }
            }

            var numHistoryItemsNotAssociated = historyItems.Count(v => v.EventType == MigrationHistoryEventType.DownloadFolderImported &&
                                                                       v.Data.GetValueOrDefault("downloadClientId") == null);

            if (numHistoryItemsNotAssociated == 0)
            {
                return;
            }

            var historyItemsToAssociate = new Dictionary<MigrationHistoryItem, MigrationHistoryItem>();

            var historyItemsLookup = historyItems.ToLookup(v => v.EpisodeId);

            foreach (var historyItemGroup in historyItemsLookup)
            {
                var list = historyItemGroup.ToList();

                for (int i = 0; i < list.Count - 1; i++)
                {
                    var grabbedEvent = list[i];
                    if (grabbedEvent.EventType != MigrationHistoryEventType.Grabbed)
                    {
                        continue;
                    }

                    if (grabbedEvent.Data.GetValueOrDefault("downloadClient") == null || grabbedEvent.Data.GetValueOrDefault("downloadClientId") == null)
                    {
                        continue;
                    }

                    // Check if it is already associated with a failed/imported event.
                    int j;
                    for (j = i + 1; j < list.Count; j++)
                    {
                        if (list[j].EventType != MigrationHistoryEventType.DownloadFolderImported &&
                            list[j].EventType != MigrationHistoryEventType.DownloadFailed)
                        {
                            continue;
                        }

                        if (list[j].Data.ContainsKey("downloadClient") && list[j].Data["downloadClient"] == grabbedEvent.Data["downloadClient"] &&
                            list[j].Data.ContainsKey("downloadClientId") && list[j].Data["downloadClientId"] == grabbedEvent.Data["downloadClientId"])
                        {
                            break;
                        }
                    }

                    if (j != list.Count)
                    {
                        list.RemoveAt(j);
                        list.RemoveAt(i--);
                        continue;
                    }

                    var importedEvent = list[i + 1];
                    if (importedEvent.EventType != MigrationHistoryEventType.DownloadFolderImported)
                    {
                        continue;
                    }

                    var droppedPath = importedEvent.Data.GetValueOrDefault("droppedPath");
                    if (droppedPath != null && new FileInfo(droppedPath).Directory.Name == grabbedEvent.SourceTitle)
                    {
                        historyItemsToAssociate[importedEvent] = grabbedEvent;

                        list.RemoveAt(i + 1);
                        list.RemoveAt(i--);
                    }
                }
            }

            foreach (var pair in historyItemsToAssociate)
            {
                using (IDbCommand updateHistoryCmd = conn.CreateCommand())
                {
                    pair.Key.Data["downloadClient"] = pair.Value.Data["downloadClient"];
                    pair.Key.Data["downloadClientId"] = pair.Value.Data["downloadClientId"];

                    updateHistoryCmd.Transaction = tran;
                    updateHistoryCmd.CommandText = "UPDATE History SET Data = ? WHERE Id = ?";
                    updateHistoryCmd.AddParameter(pair.Key.Data.ToJson());
                    updateHistoryCmd.AddParameter(pair.Key.Id);

                    updateHistoryCmd.ExecuteNonQuery();
                }
            }

            _logger.Info("Updated old History items. {0}/{1} old ImportedEvents were associated with GrabbedEvents.", historyItemsToAssociate.Count, numHistoryItemsNotAssociated);
        }
    }
}
