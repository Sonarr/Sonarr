using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Torznab;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    [TestFixture]
    public class SeedConfigProviderFixture : CoreTest<SeedConfigProvider>
    {
        [Test]
        public void should_not_return_config_for_non_existent_indexer()
        {
            Mocker.GetMock<IIndexerFactory>()
                  .Setup(v => v.Get(It.IsAny<int>()))
                  .Throws(new ModelNotFoundException(typeof(IndexerDefinition), 0));

            var result = Subject.GetSeedConfiguration(new RemoteEpisode
            {
                Release = new ReleaseInfo()
                {
                    DownloadProtocol = DownloadProtocol.Torrent,
                    IndexerId = 0
                }
            });

            result.Should().BeNull();
        }

        [Test]
        public void should_return_season_time_for_season_packs()
        {
            var settings = new TorznabSettings();
            settings.SeedCriteria.SeasonPackSeedTime = 10;

            Mocker.GetMock<IIndexerFactory>()
                     .Setup(v => v.Get(It.IsAny<int>()))
                     .Returns(new IndexerDefinition
                     {
                         Settings = settings
                     });

            var result = Subject.GetSeedConfiguration(new RemoteEpisode
            {
                Release = new ReleaseInfo()
                {
                    DownloadProtocol = DownloadProtocol.Torrent,
                    IndexerId = 1
                },
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    FullSeason = true
                }
            });

            result.Should().NotBeNull();
            result.SeedTime.Should().Be(TimeSpan.FromMinutes(10));
        }
    }
}
