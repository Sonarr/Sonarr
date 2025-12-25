using System.Threading.Tasks;
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
        private async Task GivenIndexers()
        {
            var indexers = Builder<IndexerDefinition>.CreateListOfSize(2)
                .All()
                .With(c => c.Id = 0)
                .TheFirst(1)
                .With(x => x.Name = "MyIndexer (Prowlarr)")
                .TheNext(1)
                .With(x => x.Name = "My Second Indexer (Prowlarr)")
                .BuildList();

            await Subject.InsertManyAsync(indexers);
        }

        [Test]
        public async Task should_finds_with_name()
        {
            await GivenIndexers();
            var found = await Subject.FindByNameAsync("MyIndexer (Prowlarr)");
            found.Should().NotBeNull();
            found.Name.Should().Be("MyIndexer (Prowlarr)");
            found.Id.Should().Be(1);
        }

        [Test]
        public async Task should_not_find_with_incorrect_case_name()
        {
            await GivenIndexers();
            var found = await Subject.FindByNameAsync("myindexer (prowlarr)");
            found.Should().BeNull();
        }
    }
}
