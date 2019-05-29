using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_download_client_priorityFixture : MigrationTest<add_download_client_priority>
    {
        [Test]
        public void should_set_prio_to_one()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Deluge",
                    Implementation = "Deluge",
                    Settings = new DelugeSettings85
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/"
                    }.ToJson(),
                    ConfigContract = "DelugeSettings"
                });
            });

            var items = db.Query<DownloadClientDefinition132>("SELECT * FROM DownloadClients");

            items.Should().HaveCount(1);
            items.First().Priority.Should().Be(1);
        }

        [Test]
        public void should_renumber_prio_for_enabled_clients()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Deluge",
                    Implementation = "Deluge",
                    Settings = new DelugeSettings85
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/"
                    }.ToJson(),
                    ConfigContract = "DelugeSettings"
                }).Row(new
                {
                    Enable = 1,
                    Name = "Deluge2",
                    Implementation = "Deluge",
                    Settings = new DelugeSettings85
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/"
                    }.ToJson(),
                    ConfigContract = "DelugeSettings"
                }).Row(new
                {
                    Enable = 1,
                    Name = "sab",
                    Implementation = "Sabnzbd",
                    Settings = new SabnzbdSettings81
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc"
                    }.ToJson(),
                    ConfigContract = "SabnzbdSettings"
                });
            });

            var items = db.Query<DownloadClientDefinition132>("SELECT * FROM DownloadClients");

            items.Should().HaveCount(3);
            items[0].Priority.Should().Be(1);
            items[1].Priority.Should().Be(2);
            items[2].Priority.Should().Be(1);
        }

        [Test]
        public void should_not_renumber_prio_for_disabled_clients()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 0,
                    Name = "Deluge",
                    Implementation = "Deluge",
                    Settings = new DelugeSettings85
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/"
                    }.ToJson(),
                    ConfigContract = "DelugeSettings"
                }).Row(new
                {
                    Enable = 0,
                    Name = "Deluge2",
                    Implementation = "Deluge",
                    Settings = new DelugeSettings85
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/"
                    }.ToJson(),
                    ConfigContract = "DelugeSettings"
                }).Row(new
                {
                    Enable = 0,
                    Name = "sab",
                    Implementation = "Sabnzbd",
                    Settings = new SabnzbdSettings81
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc"
                    }.ToJson(),
                    ConfigContract = "SabnzbdSettings"
                });
            });

            var items = db.Query<DownloadClientDefinition132>("SELECT * FROM DownloadClients");

            items.Should().HaveCount(3);
            items[0].Priority.Should().Be(1);
            items[1].Priority.Should().Be(1);
            items[1].Priority.Should().Be(1);
        }
    }

    public class DownloadClientDefinition132
    {
        public int Id { get; set; }
        public bool Enable { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public string Implementation { get; set; }
        public JObject Settings { get; set; }
        public string ConfigContract { get; set; }
    }
}
