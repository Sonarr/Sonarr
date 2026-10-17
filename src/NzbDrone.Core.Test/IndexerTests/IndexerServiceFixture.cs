using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.FileList;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class IndexerServiceFixture : DbTest<IndexerFactory, IndexerDefinition>
    {
        private List<IIndexer> _indexers;

        [SetUp]
        public void Setup()
        {
            _indexers = new List<IIndexer>();

            _indexers.Add(Mocker.Resolve<Newznab>());
            _indexers.Add(Mocker.Resolve<FileList>());

            Mocker.SetConstant<IEnumerable<IIndexer>>(_indexers);
        }

        [Test]
        public void should_remove_missing_indexers_on_startup()
        {
            var repo = Mocker.Resolve<IndexerRepository>();

            Mocker.SetConstant<IIndexerRepository>(repo);

            var existingIndexers = Builder<IndexerDefinition>.CreateNew().BuildNew();
            existingIndexers.ConfigContract = nameof(NewznabSettings);

            repo.Insert(existingIndexers);

            Subject.Handle(new ApplicationStartedEvent());

            AllStoredModels.Should().NotContain(c => c.Id == existingIndexers.Id);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_set_default_fail_downloads_on_default_definitions()
        {
            var definitions = Subject.GetDefaultDefinitions().ToList();

            definitions.Single(d => d.Implementation == nameof(Newznab)).Settings.As<NewznabSettings>().FailDownloads
                .Should().BeEquivalentTo([(int)FailDownloads.Executables, (int)FailDownloads.PotentiallyDangerous]);

            definitions.Single(d => d.Implementation == nameof(FileList)).Settings.As<FileListSettings>().FailDownloads
                .Should().BeEmpty();
        }

        [Test]
        public void should_set_default_fail_downloads_on_preset_definitions()
        {
            var definition = Subject.GetDefaultDefinitions().Single(d => d.Implementation == nameof(Newznab));
            int[] expected = [(int)FailDownloads.Executables, (int)FailDownloads.PotentiallyDangerous];

            Subject.GetPresetDefinitions(definition)
                .Should().NotBeEmpty()
                .And.OnlyContain(d => d.Settings.As<NewznabSettings>().FailDownloads.SequenceEqual(expected));
        }
    }
}
