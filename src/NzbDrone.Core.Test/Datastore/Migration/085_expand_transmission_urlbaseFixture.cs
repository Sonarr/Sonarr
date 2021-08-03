using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class expand_transmission_urlbaseFixture : MigrationTest<expand_transmission_urlbase>
    {
        [Test]
        public void should_not_fail_if_no_transmission()
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

            var items = db.Query<DownloadClientDefinition81>("SELECT * FROM DownloadClients");

            items.Should().HaveCount(1);
            items.First().Settings.ToObject<DelugeSettings85>().UrlBase.Should().Be("/my/");
        }

        [Test]
        public void should_be_updated_for_transmission()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Trans",
                    Implementation = "Transmission",
                    Settings = new TransmissionSettings81
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc"
                    }.ToJson(),
                    ConfigContract = "TransmissionSettings"
                });
            });

            var items = db.Query<DownloadClientDefinition81>("SELECT * FROM DownloadClients");

            items.Should().HaveCount(1);
            items.First().Settings.ToObject<TransmissionSettings81>().UrlBase.Should().Be("/transmission/");
        }

        [Test]
        public void should_be_append_to_existing_urlbase()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Trans",
                    Implementation = "Transmission",
                    Settings = new TransmissionSettings81
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/url/"
                    }.ToJson(),
                    ConfigContract = "TransmissionSettings"
                });
            });

            var items = db.Query<DownloadClientDefinition81>("SELECT * FROM DownloadClients");

            items.Should().HaveCount(1);
            items.First().Settings.ToObject<TransmissionSettings81>().UrlBase.Should().Be("/my/url/transmission/");
        }
    }
}
