using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Download.Clients.RTorrent;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class cdh_per_downloadclientFixture : MigrationTest<cdh_per_downloadclient>
    {
        [Test]
        public void should_set_cdh_to_enabled()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Deluge",
                    Implementation = "Deluge",
                    Priority = 1,
                    Settings = new DelugeSettings85
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/"
                    }.ToJson(),
                    ConfigContract = "DelugeSettings"
                });
            });

            var items = db.Query<DownloadClientDefinition158>("SELECT * FROM DownloadClients");

            items.Should().HaveCount(1);
            items.First().RemoveCompletedDownloads.Should().BeFalse();
            items.First().RemoveFailedDownloads.Should().BeTrue();
        }

        [Test]
        public void should_set_cdh_to_disabled_when_globally_disabled()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "removecompleteddownloads",
                    Value = "True"
                });

                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Deluge",
                    Implementation = "Deluge",
                    Priority = 1,
                    Settings = new DelugeSettings85
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/"
                    }.ToJson(),
                    ConfigContract = "DelugeSettings"
                });
            });

            var items = db.Query<DownloadClientDefinition158>("SELECT * FROM DownloadClients");

            items.Should().HaveCount(1);
            items.First().RemoveCompletedDownloads.Should().BeTrue();
            items.First().RemoveFailedDownloads.Should().BeTrue();
        }

        [Test]
        public void should_disable_remove_for_existing_rtorrent()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "RTorrent",
                    Implementation = "RTorrent",
                    Priority = 1,
                    Settings = new RTorrentSettings
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/"
                    }.ToJson(),
                    ConfigContract = "RTorrentSettings"
                });
            });

            var items = db.Query<DownloadClientDefinition158>("SELECT * FROM DownloadClients");

            items.Should().HaveCount(1);
            items.First().RemoveCompletedDownloads.Should().BeFalse();
            items.First().RemoveFailedDownloads.Should().BeTrue();
        }
    }

    public class DownloadClientDefinition158
    {
        public int Id { get; set; }
        public bool Enable { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public string Implementation { get; set; }
        public JObject Settings { get; set; }
        public string ConfigContract { get; set; }
        public bool RemoveCompletedDownloads { get; set; }
        public bool RemoveFailedDownloads { get; set; }
    }
}
