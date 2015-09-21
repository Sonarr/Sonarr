using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.Search.Common;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search
{
    [TestFixture]
    public class MediaModelSpecificationFixture : TestBase<MediaModelSpecification>
    {
        private Series _series1;
        private Series _series2;
        private Movie _movie1;
        private Movie _movie2;
        private RemoteEpisode _remoteEpisode = new RemoteEpisode();
        private SearchCriteriaBase _searchCriteria = new SingleEpisodeSearchCriteria();
        private RemoteMovie _remoteMovie = new RemoteMovie();
        private SearchCriteriaBase _searchMovieCriteria = new MovieSearchCriteria();

        [SetUp]
        public void Setup()
        {
            _series1 = Builder<Series>.CreateNew().With(s => s.Id = 1).Build();
            _series2 = Builder<Series>.CreateNew().With(s => s.Id = 2).Build();

            _movie1 = Builder<Movie>.CreateNew().With(s => s.Id = 1).Build();
            _movie2 = Builder<Movie>.CreateNew().With(s => s.Id = 2).Build();

            _remoteEpisode.Series = _series1;
            _remoteMovie.Movie = _movie1;
        }

        [Test]
        public void should_return_false_if_series_doesnt_match()
        {
            _searchCriteria.Media = _series2;
            _searchMovieCriteria.Media = _movie2;

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_remoteMovie, _searchMovieCriteria).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_series_ids_match()
        {
            _searchCriteria.Media = _series1;
            _searchMovieCriteria.Media = _movie1;

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, _searchMovieCriteria).Accepted.Should().BeTrue();
        }
    }
}
