using System;
using System.Linq;
using FluentAssertions;
using FluentMigrator;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class disable_eztvFixture : MigrationTest<Core.Datastore.Migration.disable_eztv>
    {
        [Test]
        public void should_disable_rss_for_eztv()
        {
            WithTestDb(c =>
            {
                InsertIndexer(c, "https://www.ezrss.it/");
            });

            var indexers = Mocker.Resolve<IndexerRepository>().All().ToList();
            indexers.First().EnableRss.Should().BeFalse();
        }

        [Test]
        public void should_disable_search_for_eztv()
        {
            WithTestDb(c =>
            {
                InsertIndexer(c, "https://www.ezrss.it/");
            });

            var indexers = Mocker.Resolve<IndexerRepository>().All().ToList();
            indexers.First().EnableSearch.Should().BeFalse();
        }

        [Test]
        public void should_not_disable_if_using_custom_url()
        {
            WithTestDb(c =>
            {
                InsertIndexer(c, "https://ezrss.sonarr.tv/");
            });

            var indexers = Mocker.Resolve<IndexerRepository>().All().ToList();
            indexers.First().EnableRss.Should().BeTrue();
            indexers.First().EnableSearch.Should().BeTrue();
        }
        
        private void InsertIndexer(MigrationBase migrationBase, string url)
        {
            migrationBase.Insert.IntoTable("Indexers").Row(new
            {
                Name = "eztv",
                Implementation = "Eztv",
                Settings = String.Format(@"{{
                                    ""baseUrl"": ""{0}""
                                 }}", url),
                ConfigContract = "EztvSettings",
                EnableRss = 1,
                EnableSearch = 1
            });
        }
    }
}
