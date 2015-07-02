using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.KickassTorrents;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class update_kickass_url_migration_fixture : MigrationTest<update_kickass_url>
    {
        [TestCase("http://kickass.so")]
        [TestCase("https://kickass.so")]
        [TestCase("http://kickass.to")]
        [TestCase("https://kickass.to")]
        [TestCase("http://kat.cr")]
        // [TestCase("HTTP://KICKASS.SO")] Not sure if there is an easy way to do this, not sure if worth it.
        public void should_replace_old_url(string oldUrl)
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("Indexers").Row(new 
                {
                    Name = "Kickass_wrong_url",
                    Implementation = "KickassTorrents",
                    Settings = new KickassTorrentsSettings
                    {
                        BaseUrl = oldUrl
                    }.ToJson(),
                    ConfigContract = "KickassTorrentsSettings"
                });
            });

            var items = Mocker.Resolve<IndexerRepository>().All().ToList();

            items.Should().HaveCount(1);
            items.First().Settings.As<KickassTorrentsSettings>().BaseUrl.Should().Be("https://kat.cr");
        }

        [Test]
        public void should_not_replace_other_indexers()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("Indexers").Row(new
                {
                    Name = "not_kickass",
                    Implementation = "NotKickassTorrents",
                    Settings = new KickassTorrentsSettings
                    {
                        BaseUrl = "kickass.so",
                    }.ToJson(),
                    ConfigContract = "KickassTorrentsSettings"
                });
            });

            var items = Mocker.Resolve<IndexerRepository>().All().ToList();

            items.Should().HaveCount(1);
            items.First().Settings.As<KickassTorrentsSettings>().BaseUrl.Should().Be("kickass.so");
        }
    }
}
