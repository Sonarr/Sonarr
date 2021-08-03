using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Torznab;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class IndexerJackettAllCheckFixture : CoreTest<IndexerJackettAllCheck>
    {
        private List<IndexerDefinition> _indexers = new List<IndexerDefinition>();
        private IndexerDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IIndexerFactory>()
                  .Setup(v => v.All())
                  .Returns(_indexers);
        }

        private void GivenIndexer(string baseUrl, string apiPath)
        {
            var torznabSettings = new TorznabSettings
            {
                BaseUrl = baseUrl,
                ApiPath = apiPath
            };

            _definition = new IndexerDefinition
            {
                Name = "Indexer",
                ConfigContract = "TorznabSettings",
                Settings = torznabSettings
            };

            _indexers.Add(_definition);
        }

        [Test]
        public void should_not_return_error_when_no_indexers()
        {
            Subject.Check().ShouldBeOk();
        }

        [TestCase("http://localhost:9117/", "api")]
        public void should_not_return_error_when_no_jackett_all_indexers(string baseUrl, string apiPath)
        {
            GivenIndexer(baseUrl, apiPath);

            Subject.Check().ShouldBeOk();
        }

        [TestCase("http://localhost:9117/torznab/all/api", "api")]
        [TestCase("http://localhost:9117/api/v2.0/indexers/all/results/torznab", "api")]
        [TestCase("http://localhost:9117/", "/torznab/all/api")]
        [TestCase("http://localhost:9117/", "/api/v2.0/indexers/all/results/torznab")]
        public void should_return_warning_if_any_jackett_all_indexer_exists(string baseUrl, string apiPath)
        {
            GivenIndexer(baseUrl, apiPath);

            Subject.Check().ShouldBeWarning();
        }
    }
}
