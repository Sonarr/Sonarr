using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class EpisodeTitleSpecificationFixture : CoreTest<EpisodeTitleSpecification>
    {
        private Series _series;
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Standard)
                                     .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                                     .Build();

            var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .With(e => e.AirDateUtc = DateTime.UtcNow)
                                           .Build()
                                           .ToList();

            _localEpisode = new LocalEpisode
                                {
                                    Path = @"C:\Test\Unsorted\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                                    Episodes = episodes,
                                    Series = _series
                                };

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.RequiresEpisodeTitle(_series, episodes))
                  .Returns(true);
        }

        [Test]
        public void should_reject_when_title_is_null()
        {
            _localEpisode.Episodes.First().Title = null;

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_when_title_is_TBA()
        {
            _localEpisode.Episodes.First().Title = "TBA";

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_when_file_is_in_series_folder()
        {
            _localEpisode.ExistingFile = true;
            _localEpisode.Episodes.First().Title = "TBA";

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_did_not_air_recently_but_title_is_TBA()
        {
            _localEpisode.Episodes.First().AirDateUtc = DateTime.UtcNow.AddDays(-7);
            _localEpisode.Episodes.First().Title = "TBA";

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_episode_title_is_not_required()
        {
            _localEpisode.Episodes.First().Title = "TBA";

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.RequiresEpisodeTitle(_series, _localEpisode.Episodes))
                  .Returns(false);

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_episode_title_is_never_required()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.EpisodeTitleRequired)
                  .Returns(EpisodeTitleRequiredType.Never);

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_title_is_required_for_bulk_season_releases_and_not_bulk_season()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.EpisodeTitleRequired)
                  .Returns(EpisodeTitleRequiredType.BulkSeasonReleases);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(Builder<Episode>.CreateListOfSize(5).BuildList());

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_title_is_required_for_bulk_season_releases()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.EpisodeTitleRequired)
                  .Returns(EpisodeTitleRequiredType.BulkSeasonReleases);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(Builder<Episode>.CreateListOfSize(5)
                                           .All()
                                           .With(e => e.AirDateUtc == _localEpisode.Episodes.First().AirDateUtc)
                                           .BuildList());

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_if_episode_title_is_required_for_bulk_season_releases_and_it_is_mising()
        {
            _localEpisode.Episodes.First().Title = "TBA";

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.EpisodeTitleRequired)
                  .Returns(EpisodeTitleRequiredType.BulkSeasonReleases);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(Builder<Episode>.CreateListOfSize(5)
                                           .All()
                                           .With(e => e.AirDateUtc = _localEpisode.Episodes.First().AirDateUtc)
                                           .BuildList());

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }
    }
}
