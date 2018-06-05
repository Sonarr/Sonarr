using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class DownloadClientFixture : IntegrationTest
    {

        [Test, Order(0)]
        public void add_downloadclient_without_name_should_return_badrequest()
        {
            EnsureNoDownloadClient();

            var schema = DownloadClients.Schema().First(v => v.Implementation == "UsenetBlackhole");

            schema.Enable = true;
            schema.Fields.First(v => v.Name == "watchFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Watch");
            schema.Fields.First(v => v.Name == "nzbFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Nzb");

            DownloadClients.InvalidPost(schema);
        }

        [Test, Order(0)]
        public void add_downloadclient_without_nzbfolder_should_return_badrequest()
        {
            EnsureNoDownloadClient();

            var schema = DownloadClients.Schema().First(v => v.Implementation == "UsenetBlackhole");

            schema.Enable = true;
            schema.Name = "Test UsenetBlackhole";
            schema.Fields.First(v => v.Name == "watchFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Watch");

            DownloadClients.InvalidPost(schema);
        }

        [Test, Order(0)]
        public void add_downloadclient_without_watchfolder_should_return_badrequest()
        {
            EnsureNoDownloadClient();

            var schema = DownloadClients.Schema().First(v => v.Implementation == "UsenetBlackhole");

            schema.Enable = true;
            schema.Name = "Test UsenetBlackhole";
            schema.Fields.First(v => v.Name == "nzbFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Nzb");

            DownloadClients.InvalidPost(schema);
        }

        [Test, Order(1)]
        public void add_downloadclient()
        {
            EnsureNoDownloadClient();

            var schema = DownloadClients.Schema().First(v => v.Implementation == "UsenetBlackhole");

            schema.Enable = true;
            schema.Name = "Test UsenetBlackhole";
            schema.Fields.First(v => v.Name == "watchFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Watch");
            schema.Fields.First(v => v.Name == "nzbFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Nzb");

            var result = DownloadClients.Post(schema);

            result.Enable.Should().BeTrue();
        }

        [Test, Order(2)]
        public void get_all_downloadclients()
        {
            EnsureDownloadClient();

            var clients = DownloadClients.All();

            clients.Should().NotBeNullOrEmpty();
        }

        [Test, Order(2)]
        public void get_downloadclient_by_id()
        {
            var client = EnsureDownloadClient();

            var result = DownloadClients.Get(client.Id);

            result.Should().NotBeNull();
        }

        [Test]
        public void get_downloadclient_by_unknown_id_should_return_404()
        {
            var result = DownloadClients.InvalidGet(1000000);
        }

        [Test, Order(3)]
        public void update_downloadclient()
        {
            EnsureNoDownloadClient();
            var client = EnsureDownloadClient();

            client.Fields.First(v => v.Name == "nzbFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Nzb2");
            var result = DownloadClients.Put(client);

            result.Should().NotBeNull();
        }

        [Test, Order(4)]
        public void delete_downloadclient()
        {
            var client = EnsureDownloadClient();

            DownloadClients.Get(client.Id).Should().NotBeNull();

            DownloadClients.Delete(client.Id);

            DownloadClients.All().Should().NotContain(v => v.Id == client.Id);
        }
    }
}
