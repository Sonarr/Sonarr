using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.Common;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class QualityAllowedByProfileSpecificationFixture : CoreTest<QualityAllowedByProfileSpecification>
    {
        private RemoteEpisode remoteEpisode;

        public static object[] AllowedTestCases =
        {
            new object[] { Quality.DVD },
            new object[] { Quality.HDTV720p },
            new object[] { Quality.Bluray1080p }
        };

        public static object[] DeniedTestCases =
        {
            new object[] { Quality.SDTV },
            new object[] { Quality.WEBDL720p },
            new object[] { Quality.Bluray720p }
        };

        private RemoteMovie remoteMovie;

        [SetUp]
        public void Setup()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.Profile = (LazyLoaded<Profile>)new Profile { Cutoff = Quality.Bluray1080p })
                         .Build();

            remoteEpisode = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
            };

            var fakeMovie = Builder<Movie>.CreateNew()
                         .With(c => c.Profile = (LazyLoaded<Profile>)new Profile { Cutoff = Quality.Bluray1080p })
                         .Build();

            remoteMovie = new RemoteMovie
            {
                Movie = fakeMovie,
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) }
            };
        }

        [Test, TestCaseSource("AllowedTestCases")]
        public void should_allow_if_quality_is_defined_in_profile(Quality qualityType)
        {
            remoteEpisode.ParsedEpisodeInfo.Quality.Quality = qualityType;
            remoteEpisode.Series.Profile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p);

            remoteMovie.ParsedMovieInfo.Quality.Quality = qualityType;
            remoteMovie.Movie.Profile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p);

            Subject.IsSatisfiedBy(remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test, TestCaseSource("DeniedTestCases")]
        public void should_not_allow_if_quality_is_not_defined_in_profile(Quality qualityType)
        {
            remoteEpisode.ParsedEpisodeInfo.Quality.Quality = qualityType;
            remoteEpisode.Series.Profile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p);

            remoteMovie.ParsedMovieInfo.Quality.Quality = qualityType;
            remoteMovie.Movie.Profile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p);

            Subject.IsSatisfiedBy(remoteEpisode, null).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeFalse();
        }
    }
}