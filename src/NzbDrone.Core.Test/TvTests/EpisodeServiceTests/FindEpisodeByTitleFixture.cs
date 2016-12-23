using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeServiceTests
{
    [TestFixture]
    public class FindEpisodeByTitleFixture : CoreTest<EpisodeService>
    {
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _episodes = Builder<Episode>.CreateListOfSize(5)
                                        .Build()
                                        .ToList();
        }

        private void GivenEpisodesWithTitles(params string[] titles)
        {
            for (int i = 0; i < titles.Count(); i++)
            {
                _episodes[i].Title = titles[i];
            }

            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(s => s.GetEpisodes(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(_episodes);
        }

        [Test]
        public void should_find_episode_by_title()
        {
            const string expectedTitle = "A Journey to the Highlands";
            GivenEpisodesWithTitles(expectedTitle);

            Subject.FindEpisodeByTitle(1, 1, "Downton.Abbey.A.Journey.To.The.Highlands.720p.BluRay.x264-aAF")
                   .Title
                   .Should()
                   .Be(expectedTitle);
        }

        [Test]
        public void should_prefer_longer_match()
        {
            const string expectedTitle = "Inside The Walking Dead: Walker University";
            GivenEpisodesWithTitles("Inside The Walking Dead", expectedTitle);

            Subject.FindEpisodeByTitle(1, 1, "The.Walking.Dead.S04.Special.Inside.The.Walking.Dead.Walker.University.720p.HDTV.x264-W4F")
                   .Title
                   .Should()
                   .Be(expectedTitle);
        }

        [Test]
        public void should_return_null_when_no_match_is_found()
        {
            GivenEpisodesWithTitles();

            Subject.FindEpisodeByTitle(1, 1, "The.Walking.Dead.S04.Special.Inside.The.Walking.Dead.Walker.University.720p.HDTV.x264-W4F")
                   .Should()
                   .BeNull();
        }
    }
}
