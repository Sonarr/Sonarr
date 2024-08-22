using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Indexers
{
    [TestFixture]
    public class IndexerRepositoryFixture : DbTest<IndexerRepository, IndexerDefinition>
    {
        private void GivenIndexers()
        {
            var indexers = Builder<IndexerDefinition>.CreateListOfSize(2)
                .All()
                .With(c => c.Id = 0)
                .TheFirst(1)
                .With(x => x.Name = "MyIndexer (Prowlarr)")
                .TheNext(1)
                .With(x => x.Name = "My Second Indexer (Prowlarr)")
                .BuildList();

            Subject.InsertMany(indexers);
        }

        [Test]
        public void should_finds_with_name()
        {
            GivenIndexers();
            var found = Subject.FindByName("MyIndexer (Prowlarr)");
            found.Should().NotBeNull();
            found.Name.Should().Be("MyIndexer (Prowlarr)");
            found.Id.Should().Be(1);
        }

        [Test]
        public void should_not_find_with_incorrect_case_name()
        {
            GivenIndexers();
            var found = Subject.FindByName("myindexer (prowlarr)");
            found.Should().BeNull();
        }
    }
}
