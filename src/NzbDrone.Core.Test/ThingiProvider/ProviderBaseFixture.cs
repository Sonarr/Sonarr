using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Test.ThingiProvider
{

    public class ProviderRepositoryFixture : DbTest<IndexerRepository, IndexerDefinition>
    {
        [Test]
        public void should_read_write_download_provider()
        {
            var model = Builder<IndexerDefinition>.CreateNew().BuildNew();
            var newznabSettings = Builder<NewznabSettings>.CreateNew().Build();
            model.Settings = newznabSettings;
            Subject.Insert(model);

            var storedProvider = Subject.Single();

            storedProvider.Settings.Should().BeOfType<NewznabSettings>();

            var storedSetting = (NewznabSettings)storedProvider.Settings;

            storedSetting.ShouldHave().AllProperties().EqualTo(newznabSettings);
        }
    }
}