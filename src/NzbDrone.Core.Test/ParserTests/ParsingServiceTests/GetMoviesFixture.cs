using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetMoviesFixture : CoreTest<ParsingService>
    {

        private Movie _movie;
        private ParsedMovieInfo _parsedMovieInfo;
        private MovieSearchCriteria _movieSearchCriteria;

        [SetUp]
        public void Setup()
        {

            _movie = Builder<Movie>.CreateNew()
                .With(m => m.Title = "Titanic")
                .With(m => m.CleanTitle = "Titanic")
                .With(m => m.OriginalTitle = "Titanic")
                .Build();

            _parsedMovieInfo = new ParsedMovieInfo
            {
                Title = _movie.Title
            };

            _movieSearchCriteria = new MovieSearchCriteria
            {
                Media = _movie
            };
        }

        [Test]
        public void should_find_movie()
        {
            Subject.Map(_parsedMovieInfo);

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.FindByTitle(It.IsAny<String>()), Times.Once());
        }

        [Test]
        public void should_match_movie_with_search_criteria()
        {
            Subject.Map(_parsedMovieInfo, _movieSearchCriteria);

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.FindByTitle(It.IsAny<String>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_findMovie_when_search_criteria_match_fails()
        {
            _movieSearchCriteria.Movie.Title = "Other";
            _movieSearchCriteria.Movie.CleanTitle = "Other";
            _movieSearchCriteria.Movie.OriginalTitle = "Other";

            Subject.Map(_parsedMovieInfo, _movieSearchCriteria);

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.FindByTitle(It.IsAny<String>()), Times.Once());
        }

        [Test]
        public void should_use_passed_in_title_when_it_cannot_be_parsed()
        {
            const string title = "Titanic";

            Subject.GetMovie(title);

            Mocker.GetMock<IMovieService>()
                  .Verify(s => s.FindByTitle(title), Times.Once());
        }

        [Test]
        public void should_use_parsed_movie_title()
        {
            const string title = "Titanic.1997.720p.hdtv";

            Subject.GetMovie(title);

            Mocker.GetMock<IMovieService>()
                  .Verify(s => s.FindByTitle(Parser.Parser.ParseMovieTitle(title).Title), Times.Once());
        }

        [Test]
        public void should_fallback_to_title_without_year_and_year_when_title_lookup_fails()
        {
            const string title = "Space:1999.720p.hdtv";
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(title);

            Subject.GetMovie(title);

            Mocker.GetMock<IMovieService>()
                  .Verify(s => s.FindByTitle(parsedMovieInfo.TitleInfo.TitleWithoutYear,
                                             parsedMovieInfo.TitleInfo.Year), Times.Once());
        }
    }
}
