using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class move_dot_prefix_to_transmission_categoryFixture : MigrationTest<move_dot_prefix_to_transmission_category>
    {
        [Test]
        public void should_not_fail_if_no_transmission()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Sab",
                    Implementation = "Sabnzbd",
                    Settings = new
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc"
                    }.ToJson(),
                    ConfigContract = "SabnzbdSettings"
                });
            });

            var downloadClients = db.Query<DownloadClientDefinition81>("SELECT Settings FROM DownloadClients");

            downloadClients.Should().HaveCount(1);
            downloadClients.First().Settings.ToObject<SabnzbdSettings81>().TvCategory.Should().Be("abc");
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
                    Settings = new
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc"
                    }.ToJson(),
                    ConfigContract = "TransmissionSettings"
                });
            });

            var downloadClients = db.Query<DownloadClientDefinition81>("SELECT Settings FROM DownloadClients");

            downloadClients.Should().HaveCount(1);
            downloadClients.First().Settings.ToObject<TransmissionSettings81>().TvCategory.Should().Be(".abc");
        }

        [Test]
        public void should_leave_empty_category_untouched()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Trans",
                    Implementation = "Transmission",
                    Settings = new
                    {
                        Host = "127.0.0.1",
                        TvCategory = ""
                    }.ToJson(),
                    ConfigContract = "TransmissionSettings"
                });
            });

            var downloadClients = db.Query<DownloadClientDefinition81>("SELECT Settings FROM DownloadClients");

            downloadClients.Should().HaveCount(1);
            downloadClients.First().Settings.ToObject<TransmissionSettings81>().TvCategory.Should().Be("");
        }
    }
}
