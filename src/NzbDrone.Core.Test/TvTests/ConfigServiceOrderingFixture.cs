using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class ConfigServiceOrderingFixture : CoreTest<ConfigService>
    {
        [Test]
        public void tvdb_api_key_should_default_to_empty()
        {
            Subject.TvdbApiKey.Should().BeEmpty();
        }

        [Test]
        public void should_store_tvdb_api_key()
        {
            var testKey = "a23de24b-84bf-428c-8aff-f0dd2614ebcc";

            Subject.TvdbApiKey = testKey;

            Mocker.GetMock<IConfigRepository>()
                .Verify(c => c.Upsert("tvdbapikey", testKey), Times.Once());
        }

        [Test]
        public void indexer_order_matching_should_default_to_aired_order()
        {
            Subject.IndexerOrderMatching.Should().Be(IndexerOrderMatching.AiredOrder);
        }

        [Test]
        public void should_store_indexer_order_matching()
        {
            Subject.IndexerOrderMatching = IndexerOrderMatching.ChosenOrder;

            Mocker.GetMock<IConfigRepository>()
                .Verify(c => c.Upsert("indexerordermatching", "chosenorder"), Times.Once());
        }
    }
}
