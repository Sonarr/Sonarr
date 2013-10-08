using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Indexers.Omgwtfnzbs;
using NzbDrone.Core.Indexers.Wombles;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class IndexerServiceFixture : DbTest<IndexerFactory, IndexerDefinition>
    {
        private List<IIndexer> _indexers;

        [SetUp]
        public void Setup()
        {
            _indexers = new List<IIndexer>();

            _indexers.Add(new Newznab());
            _indexers.Add(new Omgwtfnzbs());
            _indexers.Add(new Wombles());

            Mocker.SetConstant<IEnumerable<IIndexer>>(_indexers);

        }

        [Test]
        public void should_create_default_indexer_on_startup()
        {
            IList<IndexerDefinition> storedIndexers = null;

            Mocker.GetMock<IIndexerRepository>()
                  .Setup(c => c.InsertMany(It.IsAny<IList<IndexerDefinition>>()))
                  .Callback<IList<IndexerDefinition>>(indexers => storedIndexers = indexers);

            Subject.Handle(new ApplicationStartedEvent());

            storedIndexers.Should().NotBeEmpty();
            storedIndexers.Select(c => c.Name).Should().OnlyHaveUniqueItems();
            storedIndexers.Select(c => c.Enable).Should().NotBeEmpty();
            storedIndexers.Select(c => c.Implementation).Should().NotContainNulls();
        }

        [Test]
        public void getting_list_of_indexers()
        {
            Mocker.SetConstant<IIndexerRepository>(Mocker.Resolve<IndexerRepository>());

            Subject.Handle(new ApplicationStartedEvent());

            var indexers = Subject.All().ToList();
            indexers.Should().NotBeEmpty();
            indexers.Should().NotContain(c => c.Settings == null);
            indexers.Should().NotContain(c => c.Name == null);
            indexers.Select(c => c.Name).Should().OnlyHaveUniqueItems();
        }


        [Test]
        public void should_remove_missing_indexers_on_startup()
        {
            var repo = Mocker.Resolve<IndexerRepository>();

            Mocker.SetConstant<IIndexerRepository>(repo);


            var existingIndexers = Builder<IndexerDefinition>.CreateNew().BuildNew();
            existingIndexers.ConfigContract = typeof (NewznabSettings).Name;

            repo.Insert(existingIndexers);


            Subject.Handle(new ApplicationStartedEvent());

            AllStoredModels.Should().NotContain(c => c.Id == existingIndexers.Id);
        }
    }
}