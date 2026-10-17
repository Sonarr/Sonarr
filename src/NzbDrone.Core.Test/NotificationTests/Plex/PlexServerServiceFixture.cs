using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Notifications.Plex.Server;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests.Plex
{
    public class PlexServerServiceFixture : CoreTest<PlexServerService>
    {
        private const string RootFolderPath = @"C:\Test\TV";

        private Series _series;
        private PlexServerSettings _settings;

        [SetUp]
        public void SetUp()
        {
            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());

            _series = new Series
            {
                Path = @"C:\Test\TV\Series Title".AsOsAgnostic()
            };

            _settings = new PlexServerSettings
            {
                Host = "127.0.0.1",
                Port = 32400
            };

            Mocker.GetMock<IRootFolderService>()
                .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                .Returns(RootFolderPath.AsOsAgnostic());

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.Version(_settings))
                .Returns("1.20.0.12345-abcdef");
        }

        private PlexSection GivenSection(int id)
        {
            var section = new PlexSection
            {
                Id = id
            };

            section.Locations.Add(new PlexSectionLocation
            {
                Id = id,
                Path = RootFolderPath.AsOsAgnostic()
            });

            return section;
        }

        [Test]
        public void should_update_all_sections_matching_the_series_path()
        {
            var sections = new List<PlexSection>
            {
                GivenSection(1),
                GivenSection(2)
            };

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.GetTvSections(_settings))
                .Returns(sections);

            Subject.UpdateLibrary(_series, _settings);

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(1, It.IsAny<string>(), _settings), Times.Once());

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(2, It.IsAny<string>(), _settings), Times.Once());
        }

        [Test]
        public void should_update_only_the_matching_section_when_a_single_section_matches()
        {
            var matchingSection = GivenSection(1);
            var nonMatchingSection = new PlexSection
            {
                Id = 2
            };

            nonMatchingSection.Locations.Add(new PlexSectionLocation
            {
                Id = 2,
                Path = @"C:\Test\OtherTv".AsOsAgnostic()
            });

            var sections = new List<PlexSection> { matchingSection, nonMatchingSection };

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.GetTvSections(_settings))
                .Returns(sections);

            Subject.UpdateLibrary(_series, _settings);

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(1, It.IsAny<string>(), _settings), Times.Once());

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(2, It.IsAny<string>(), _settings), Times.Never());
        }

        [Test]
        public void should_only_update_a_section_once_when_multiple_locations_match()
        {
            var section = new PlexSection { Id = 1 };

            section.Locations.Add(new PlexSectionLocation { Id = 1, Path = RootFolderPath.AsOsAgnostic() });
            section.Locations.Add(new PlexSectionLocation { Id = 2, Path = RootFolderPath.AsOsAgnostic() });

            var sections = new List<PlexSection> { section };

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.GetTvSections(_settings))
                .Returns(sections);

            Subject.UpdateLibrary(_series, _settings);

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(1, It.IsAny<string>(), _settings), Times.Once());
        }

        [Test]
        public void should_fall_back_to_updating_every_section_when_none_match()
        {
            var sections = new List<PlexSection>
            {
                new PlexSection { Id = 1 },
                new PlexSection { Id = 2 }
            };

            sections[0].Locations.Add(new PlexSectionLocation { Id = 1, Path = @"C:\Test\OtherTv".AsOsAgnostic() });
            sections[1].Locations.Add(new PlexSectionLocation { Id = 2, Path = @"C:\Test\OtherTv2".AsOsAgnostic() });

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.GetTvSections(_settings))
                .Returns(sections);

            Subject.UpdateLibrary(_series, _settings);

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(1, It.IsAny<string>(), _settings), Times.Once());

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(2, It.IsAny<string>(), _settings), Times.Once());
        }
    }
}
