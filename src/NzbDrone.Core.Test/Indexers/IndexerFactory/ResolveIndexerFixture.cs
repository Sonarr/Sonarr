using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Indexer
{
    [TestFixture]
    public class ResolveIndexerFixture : CoreTest<IndexerFactory>
    {
        private List<IndexerDefinition> _indexers;

        [SetUp]
        public void SetUp()
        {
            _indexers = Builder<IndexerDefinition>.CreateListOfSize(3)
                .Build()
                .ToList();

            Mocker.GetMock<IIndexerRepository>()
                .Setup(v => v.All())
                .Returns(_indexers);
        }

        [Test]
        public void should_throw_if_indexer_with_id_cannot_be_found()
        {
            Assert.Throws<ResolveIndexerException>(() => Subject.ResolveIndexer(10, null));
        }

        [Test]
        public void should_throw_if_indexer_with_name_cannot_be_found()
        {
            Assert.Throws<ResolveIndexerException>(() => Subject.ResolveIndexer(null, "Not a Real Client"));
        }

        [Test]
        public void should_throw_if_indexer_with_id_does_not_match_indexer_with_name()
        {
            Assert.Throws<ResolveIndexerException>(() => Subject.ResolveIndexer(_indexers[0].Id, _indexers[1].Name));
        }

        [Test]
        public void should_return_indexer_when_only_id_is_provided()
        {
            var result = Subject.ResolveIndexer(_indexers[0].Id, null);

            result.Should().NotBeNull();
            result.Should().Be(_indexers[0]);
        }

        [Test]
        public void should_return_indexer_when_only_name_is_provided()
        {
            var result = Subject.ResolveIndexer(null, _indexers[0].Name);

            result.Should().NotBeNull();
            result.Should().Be(_indexers[0]);
        }

        [Test]
        public void should_return_indexer_when_id_and_name_provided_for_the_same_indexer()
        {
            var result = Subject.ResolveIndexer(_indexers[0].Id, _indexers[0].Name);

            result.Should().NotBeNull();
            result.Should().Be(_indexers[0]);
        }

        [Test]
        public void should_return_null_if_both_id_and_name_are_not_provided()
        {
            var result = Subject.ResolveIndexer(null, null);

            result.Should().BeNull();
        }
    }
}
