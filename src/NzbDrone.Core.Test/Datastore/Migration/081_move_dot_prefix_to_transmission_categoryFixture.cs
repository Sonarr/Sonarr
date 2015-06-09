using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Download.Clients.Transmission;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class move_dot_prefix_to_transmission_categoryFixture : MigrationTest<Core.Datastore.Migration.move_dot_prefix_to_transmission_category>
    {
        [Test]
        public void should_not_fail_if_no_transmission()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new 
                {
                    Enable = 1,
                    Name = "Sab",
                    Implementation = "Sabnzbd",
                    Settings = new SabnzbdSettings
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc"
                    }.ToJson(),
                    ConfigContract = "SabnzbdSettings"
                });
            });

            var items = Mocker.Resolve<DownloadClientRepository>().All();

            items.Should().HaveCount(1);

            items.First().Settings.As<SabnzbdSettings>().TvCategory.Should().Be("abc");
        }

        [Test]
        public void should_be_updated_for_transmission()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Trans",
                    Implementation = "Transmission",
                    Settings = new TransmissionSettings
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc"
                    }.ToJson(),
                    ConfigContract = "TransmissionSettings"
                });
            });

            var items = Mocker.Resolve<DownloadClientRepository>().All();

            items.Should().HaveCount(1);

            items.First().Settings.As<TransmissionSettings>().TvCategory.Should().Be(".abc");
        }

        [Test]
        public void should_leave_empty_category_untouched()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new
                {
                    Enable = 1,
                    Name = "Trans",
                    Implementation = "Transmission",
                    Settings = new TransmissionSettings
                    {
                        Host = "127.0.0.1",
                        TvCategory = ""
                    }.ToJson(),
                    ConfigContract = "TransmissionSettings"
                });
            });

            var items = Mocker.Resolve<DownloadClientRepository>().All();

            items.Should().HaveCount(1);

            items.First().Settings.As<TransmissionSettings>().TvCategory.Should().Be("");
        }
    }
}
