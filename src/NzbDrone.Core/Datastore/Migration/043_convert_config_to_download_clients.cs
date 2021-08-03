using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(43)]
    public class convert_config_to_download_clients : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertToThingyProvder);
        }

        private void ConvertToThingyProvder(IDbConnection conn, IDbTransaction tran)
        {
            var config = new Dictionary<string, string>();

            using (IDbCommand configCmd = conn.CreateCommand())
            {
                configCmd.Transaction = tran;
                configCmd.CommandText = @"SELECT * FROM Config";
                using (IDataReader configReader = configCmd.ExecuteReader())
                {
                    var keyIndex = configReader.GetOrdinal("Key");
                    var valueIndex = configReader.GetOrdinal("Value");

                    while (configReader.Read())
                    {
                        var key = configReader.GetString(keyIndex);
                        var value = configReader.GetString(valueIndex);

                        config.Add(key.ToLowerInvariant(), value);
                    }
                }
            }

            var client = GetConfigValue(config, "DownloadClient", "");

            if (string.IsNullOrWhiteSpace(client))
            {
                return;
            }

            if (client.Equals("sabnzbd", StringComparison.InvariantCultureIgnoreCase))
            {
                var settings = new ClientSettingsForMigration
                               {
                                   Host = GetConfigValue(config, "SabHost", "localhost"),
                                   Port = GetConfigValue(config, "SabPort", 8080),
                                   ApiKey = GetConfigValue(config, "SabApiKey", ""),
                                   Username = GetConfigValue(config, "SabUsername", ""),
                                   Password = GetConfigValue(config, "SabPassword", ""),
                                   TvCategory = GetConfigValue(config, "SabTvCategory", "tv"),
                                   RecentTvPriority = GetSabnzbdPriority(GetConfigValue(config, "NzbgetRecentTvPriority", "Default")),
                                   OlderTvPriority = GetSabnzbdPriority(GetConfigValue(config, "NzbgetOlderTvPriority", "Default")),
                                   UseSsl = GetConfigValue(config, "SabUseSsl", false)
                               };

                AddDownloadClient(conn, tran, "Sabnzbd", "Sabnzbd", settings.ToJson(), "SabnzbdSettings", 1);
            }
            else if (client.Equals("nzbget", StringComparison.InvariantCultureIgnoreCase))
            {
                var settings = new ClientSettingsForMigration
                {
                    Host = GetConfigValue(config, "NzbGetHost", "localhost"),
                    Port = GetConfigValue(config, "NzbgetPort", 6789),
                    Username = GetConfigValue(config, "NzbgetUsername", "nzbget"),
                    Password = GetConfigValue(config, "NzbgetPassword", ""),
                    TvCategory = GetConfigValue(config, "NzbgetTvCategory", "tv"),
                    RecentTvPriority = GetNzbgetPriority(GetConfigValue(config, "NzbgetRecentTvPriority", "Normal")),
                    OlderTvPriority = GetNzbgetPriority(GetConfigValue(config, "NzbgetOlderTvPriority", "Normal")),
                };

                AddDownloadClient(conn, tran, "Nzbget", "Nzbget", settings.ToJson(), "NzbgetSettings", 1);
            }
            else if (client.Equals("pneumatic", StringComparison.InvariantCultureIgnoreCase))
            {
                var settings = new FolderSettingsForMigration
                               {
                                   Folder = GetConfigValue(config, "PneumaticFolder", "")
                               };

                AddDownloadClient(conn, tran, "Pneumatic", "Pneumatic", settings.ToJson(), "FolderSettings", 1);
            }
            else if (client.Equals("blackhole", StringComparison.InvariantCultureIgnoreCase))
            {
                var settings = new FolderSettingsForMigration
                {
                    Folder = GetConfigValue(config, "BlackholeFolder", "")
                };

                AddDownloadClient(conn, tran, "Blackhole", "Blackhole", settings.ToJson(), "FolderSettings", 1);
            }

            DeleteOldConfigValues(conn, tran);
        }

        private T GetConfigValue<T>(Dictionary<string, string> config, string key, T defaultValue)
        {
            key = key.ToLowerInvariant();

            if (config.ContainsKey(key))
            {
                return (T)Convert.ChangeType(config[key], typeof(T));
            }

            return defaultValue;
        }

        private void AddDownloadClient(IDbConnection conn,
            IDbTransaction tran,
            string name,
            string implementation,
            string settings,
            string configContract,
            int protocol)
        {
            using (IDbCommand updateCmd = conn.CreateCommand())
            {
                var text = string.Format("INSERT INTO DownloadClients (Enable, Name, Implementation, Settings, ConfigContract, Protocol) VALUES (1, ?, ?, ?, ?, ?)");
                updateCmd.AddParameter(name);
                updateCmd.AddParameter(implementation);
                updateCmd.AddParameter(settings);
                updateCmd.AddParameter(configContract);
                updateCmd.AddParameter(protocol);

                updateCmd.Transaction = tran;
                updateCmd.CommandText = text;
                updateCmd.ExecuteNonQuery();
            }
        }

        private void DeleteOldConfigValues(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand updateCmd = conn.CreateCommand())
            {
                var text = "DELETE FROM Config WHERE [KEY] IN ('nzbgetusername', 'nzbgetpassword', 'nzbgethost', 'nzbgetport', " +
                           "'nzbgettvcategory', 'nzbgetrecenttvpriority', 'nzbgetoldertvpriority', 'sabhost', 'sabport', " +
                           "'sabapikey', 'sabusername', 'sabpassword', 'sabtvcategory', 'sabrecenttvpriority', " +
                           "'saboldertvpriority', 'sabusessl', 'downloadclient', 'blackholefolder', 'pneumaticfolder')";

                updateCmd.Transaction = tran;
                updateCmd.CommandText = text;
                updateCmd.ExecuteNonQuery();
            }
        }

        private int GetSabnzbdPriority(string priority)
        {
            return (int)Enum.Parse(typeof(SabnzbdPriorityForMigration), priority, true);
        }

        private int GetNzbgetPriority(string priority)
        {
            return (int)Enum.Parse(typeof(NzbGetPriorityForMigration), priority, true);
        }

        private class ClientSettingsForMigration
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

        private class FolderSettingsForMigration
        {
            public string Folder { get; set; }
        }

        private enum SabnzbdPriorityForMigration
        {
            Default = -100,
            Paused = -2,
            Low = -1,
            Normal = 0,
            High = 1,
            Force = 2
        }

        private enum NzbGetPriorityForMigration
        {
            VeryLow = -100,
            Low = -50,
            Normal = 0,
            High = 50,
            VeryHigh = 100
        }
    }
}
