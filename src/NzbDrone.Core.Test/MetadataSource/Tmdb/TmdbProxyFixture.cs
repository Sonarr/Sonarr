using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource.Tmdb;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class TmdbProxyFixture : CoreTest<TmdbProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [TestCase(597, "Titanic")]
        [TestCase(9738, "Fantastic Four")]
        [TestCase(24428, "The Avengers")]
        public void should_be_able_to_get_series_detail(int tmdbId, string title)
        {
            var details = Subject.GetMovieInfo(tmdbId);

            ValidateMovie(details);

            details.Title.Should().Be(title);
        }

        [Test]
        public void getting_details_of_invalid_series()
        {
            Assert.Throws<MovieNotFoundException>(() => Subject.GetMovieInfo(Int32.MaxValue));
        }


        private void ValidateMovie(Movie movie)
        {
            movie.Should().NotBeNull();
            movie.Title.Should().NotBeNullOrWhiteSpace();
            movie.CleanTitle.Should().Be(Parser.Parser.CleanSeriesTitle(movie.Title));
            movie.Overview.Should().NotBeNullOrWhiteSpace();
            movie.ReleaseDate.Should().BeBefore(DateTime.Today);
            movie.Images.Should().NotBeEmpty();
            movie.ImdbId.Should().NotBeNullOrWhiteSpace();
            movie.Runtime.Should().BeGreaterThan(0);
            movie.TmdbId.Should().BeGreaterThan(0);
            movie.Year.Should().BeGreaterThan(0);
        }
    }
}
