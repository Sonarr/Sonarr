using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class DownloadClientFixture : IntegrationTest
    {
        [Test]
        public void should_be_able_to_add()
        {
            var schema = DownloadClients.Schema().First(v => v.Implementation == "UsenetBlackhole");

            schema.Enable = true;
            schema.Name = "Test UsenetBlackhole";
            schema.Fields.First(v => v.Name == "WatchFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Watch");
            schema.Fields.First(v => v.Name == "NzbFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Nzb");

            var result = DownloadClients.Post(schema);

            result.Enable.Should().BeTrue();
        }

        [Test]
        public void should_be_able_to_get()
        {
            Assert.Ignore("TODO");
        }

        [Test]
        public void should_be_able_to_get_by_id()
        {
            Assert.Ignore("TODO");
        }

        [Test]
        public void should_be_enabled()
        {
            Assert.Ignore("TODO");
        }
    }
}