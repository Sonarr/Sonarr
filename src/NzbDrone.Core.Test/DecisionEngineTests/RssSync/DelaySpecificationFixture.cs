using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles.Movies;
using NzbDrone.Core.MediaFiles.Series;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class DelaySpecificationFixture : CoreTest<DelaySpecification>
    {
        private Profile _profile;
        private DelayProfile _delayProfile;
        private RemoteEpisode _remoteEpisode;
        private RemoteMovie _remoteMovie;

        [SetUp]
        public void Setup()
        {
            _profile = Builder<Profile>.CreateNew()
                                       .Build();

            _delayProfile = Builder<DelayProfile>.CreateNew()
                                                 .With(d => d.PreferredProtocol = DownloadProtocol.Usenet)
                                                 .Build();

            var series = Builder<Series>.CreateNew()
                                        .With(s => s.Profile = _profile)
                                        .Build();

            _remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                   .With(r => r.Series = series)
                                                   .Build();

            var movie = Builder<Movie>.CreateNew()
                                      .With(m => m.Profile = _profile)
                                      .Build();

            _remoteMovie = Builder<RemoteMovie>.CreateNew()
                                               .With(r => r.Movie = movie)
                                               .Build();

            _profile.Items = new List<ProfileQualityItem>();
            _profile.Items.Add(new ProfileQualityItem { Allowed = true, Quality = Quality.HDTV720p });
            _profile.Items.Add(new ProfileQualityItem { Allowed = true, Quality = Quality.WEBDL720p });
            _profile.Items.Add(new ProfileQualityItem { Allowed = true, Quality = Quality.Bluray720p });

            _profile.Cutoff = Quality.WEBDL720p;

            _remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            _remoteEpisode.Release = new ReleaseInfo();
            _remoteEpisode.Release.DownloadProtocol = DownloadProtocol.Usenet;

            _remoteEpisode.Episodes = Builder<Episode>.CreateListOfSize(1).Build().ToList();
            _remoteEpisode.Episodes.First().EpisodeFileId = 0;

            _remoteMovie.ParsedMovieInfo = new ParsedMovieInfo();
            _remoteMovie.Release = new ReleaseInfo();
            _remoteMovie.Release.DownloadProtocol = DownloadProtocol.Usenet;

            _remoteMovie.Movie = movie;
            _remoteMovie.Movie.MovieFileId = 0;

            Mocker.GetMock<IDelayProfileService>()
                  .Setup(s => s.BestForTags(It.IsAny<HashSet<int>>()))
                  .Returns(_delayProfile);
        }

        private void GivenExistingFile(QualityModel quality)
        {
            _remoteEpisode.Episodes.First().EpisodeFileId = 1;

            _remoteEpisode.Episodes.First().EpisodeFile = new LazyLoaded<EpisodeFile>(new EpisodeFile
                                                                                 {
                                                                                     Quality = quality
                                                                                 });
        }

        private void GivenExistingMovie(QualityModel quality)
        {
            _remoteMovie.Movie.MovieFileId = 1;
            _remoteMovie.Movie.MovieFile = new LazyLoaded<MovieFile>(new MovieFile { Quality = quality });
        }

        private void GivenUpgradeForExistingFile()
        {
            Mocker.GetMock<IQualityUpgradableSpecification>()
                  .Setup(s => s.IsUpgradable(It.IsAny<Profile>(), It.IsAny<QualityModel>(), It.IsAny<QualityModel>()))
                  .Returns(true);
        }

        [Test]
        public void should_be_true_when_search()
        {
            Subject.IsSatisfiedBy(new RemoteEpisode(), new SingleEpisodeSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_search_movie()
        {
            Subject.IsSatisfiedBy(new RemoteMovie(), new MovieSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_profile_does_not_have_a_delay()
        {
            _delayProfile.UsenetDelay = 0;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_quality_is_last_allowed_in_profile()
        {
            _remoteEpisode.ParsedEpisodeInfo.Quality = new QualityModel(Quality.Bluray720p);
            _remoteMovie.ParsedMovieInfo.Quality = new QualityModel(Quality.Bluray720p);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_release_is_older_than_delay()
        {
            _remoteEpisode.ParsedEpisodeInfo.Quality = new QualityModel(Quality.HDTV720p);
            _remoteEpisode.Release.PublishDate = DateTime.UtcNow.AddHours(-10);

            _remoteMovie.ParsedMovieInfo.Quality = new QualityModel(Quality.HDTV720p);
            _remoteMovie.Release.PublishDate = DateTime.UtcNow.AddHours(-10);

            _delayProfile.UsenetDelay = 60;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_release_is_younger_than_delay()
        {
            _remoteEpisode.ParsedEpisodeInfo.Quality = new QualityModel(Quality.SDTV);
            _remoteEpisode.Release.PublishDate = DateTime.UtcNow;
            _remoteMovie.ParsedMovieInfo.Quality = new QualityModel(Quality.SDTV);
            _remoteMovie.Release.PublishDate = DateTime.Now;

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_release_is_a_proper_for_existing_episode()
        {
            _remoteEpisode.ParsedEpisodeInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2));
            _remoteEpisode.Release.PublishDate = DateTime.UtcNow;

            _remoteMovie.ParsedMovieInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2));
            _remoteMovie.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.HDTV720p));
            GivenUpgradeForExistingFile();

            GivenExistingMovie(new QualityModel(Quality.HDTV720p));

            Mocker.GetMock<IQualityUpgradableSpecification>()
                  .Setup(s => s.IsRevisionUpgrade(It.IsAny<QualityModel>(), It.IsAny<QualityModel>()))
                  .Returns(true);

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_release_is_a_real_for_existing_episode()
        {
            _remoteEpisode.ParsedEpisodeInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(real: 1));
            _remoteEpisode.Release.PublishDate = DateTime.UtcNow;
            _remoteMovie.ParsedMovieInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(real: 1));
            _remoteMovie.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.HDTV720p));
            GivenUpgradeForExistingFile();

            GivenExistingMovie(new QualityModel(Quality.HDTV720p));

            Mocker.GetMock<IQualityUpgradableSpecification>()
                  .Setup(s => s.IsRevisionUpgrade(It.IsAny<QualityModel>(), It.IsAny<QualityModel>()))
                  .Returns(true);

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_release_is_proper_for_existing_episode_of_different_quality()
        {
            _remoteEpisode.ParsedEpisodeInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2));
            _remoteEpisode.Release.PublishDate = DateTime.UtcNow;

            _remoteMovie.ParsedMovieInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2));
            _remoteMovie.Release.PublishDate = DateTime.UtcNow;
            
            GivenExistingFile(new QualityModel(Quality.SDTV));
            GivenExistingMovie(new QualityModel(Quality.SDTV));

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }
    }
}
