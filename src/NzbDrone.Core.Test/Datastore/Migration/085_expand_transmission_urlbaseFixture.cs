using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Deluge;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Download.Clients.Transmission;
using NzbDrone.Core.Test.Framework;
using System.Drawing;
namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class expand_transmission_urlbaseFixture : MigrationTest<Core.Datastore.Migration.expand_transmission_urlbase>
    {
        [Test]
        public void should_not_fail_if_no_transmission()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("DownloadClients").Row(new 
                {
                    Enable = 1,
                    Name = "Deluge",
                    Implementation = "Deluge",
                    Settings = new DelugeSettings
                    {
                        Host = "127.0.0.1",
                        TvCategory = "abc",
                        UrlBase = "/my/"
                    }.ToJson(),
                    ConfigContract = "DelugeSettings"
                });
            });

            var items = Mocker.Resolve<DownloadClientRepository>().All();

            items.Should().HaveCount(1);

            items.First().Settings.As<DelugeSettings>().UrlBase.Should().Be("/my/");
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
                        TvCategory = "abc",
                        UrlBase = null
                    }.ToJson(),
                    ConfigContract = "TransmissionSettings"
                });
            });

            var items = Mocker.Resolve<DownloadClientRepository>().All();

            items.Should().HaveCount(1);

            items.First().Settings.As<TransmissionSettings>().UrlBase.Should().Be("/transmission/");
        }

        [Test]
        public void should_be_append_to_existing_urlbase()
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
                        TvCategory = "abc",
                        UrlBase = "/my/url/"
                    }.ToJson(),
                    ConfigContract = "TransmissionSettings"
                });
            });

            var items = Mocker.Resolve<DownloadClientRepository>().All();

            items.Should().HaveCount(1);

            items.First().Settings.As<TransmissionSettings>().UrlBase.Should().Be("/my/url/transmission/");
        }
    }
}
