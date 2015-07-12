using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AnimeVersionUpgradeSpecificationFixture : CoreTest<AnimeVersionUpgradeSpecification>
    {
        private AnimeVersionUpgradeSpecification _subject;
        private RemoteEpisode _remoteEpisode;
        private EpisodeFile _episodeFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();
            _subject = Mocker.Resolve<AnimeVersionUpgradeSpecification>();

            _episodeFile = new EpisodeFile
                           {
                               Quality = new QualityModel(Quality.HDTV720p, new Revision()),
                               ReleaseGroup = "DRONE2"
                           };

            _remoteEpisode = new RemoteEpisode();
            _remoteEpisode.Series = new Series { SeriesType = SeriesTypes.Anime };
            _remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                               {
                                                   Quality = new QualityModel(Quality.HDTV720p, new Revision(2)),
                                                   ReleaseGroup = "DRONE"
                                               };

            _remoteEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                      .All()
                                                      .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(_episodeFile))
                                                      .Build()
                                                      .ToList();
        }

        private void GivenStandardSeries()
        {
            _remoteEpisode.Series.SeriesType = SeriesTypes.Standard;
        }

        private void GivenNoVersionUpgrade()
        {
            _remoteEpisode.ParsedEpisodeInfo.Quality.Revision = new Revision();
        }

        [Test]
        public void should_be_true_when_no_existing_file()
        {
            _remoteEpisode.Episodes.First().EpisodeFileId = 0;

            _subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_if_series_is_not_anime()
        {
            GivenStandardSeries();
            _subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_if_is_not_a_version_upgrade_for_existing_file()
        {
            GivenNoVersionUpgrade();
            _subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_release_group_matches()
        {
            _episodeFile.ReleaseGroup = _remoteEpisode.ParsedEpisodeInfo.ReleaseGroup;

            _subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_existing_file_doesnt_have_a_release_group()
        {
            _episodeFile.ReleaseGroup = string.Empty;
            _subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_should_be_false_when_release_doesnt_have_a_release_group()
        {
            _remoteEpisode.ParsedEpisodeInfo.ReleaseGroup = string.Empty;
            _subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_when_release_group_does_not_match()
        {
            _subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }
    }
}