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
            for (int i = 0; i < titles.Length; i++)
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

        [Test]
        public void should_handle_e00_specials()
        {
            const string expectedTitle = "Inside The Walking Dead: Walker University";
            GivenEpisodesWithTitles("Inside The Walking Dead", expectedTitle, "Inside The Walking Dead Walker University 2");

            Subject.FindEpisodeByTitle(1, 1, "The.Walking.Dead.S04E00.Inside.The.Walking.Dead.Walker.University.720p.HDTV.x264-W4F")
                   .Title
                   .Should()
                   .Be(expectedTitle);
        }

        [TestCase("Dead.Man.Walking.S04E00.Inside.The.Walking.Dead.Walker.University.720p.HDTV.x264-W4F", "Inside The Walking Dead: Walker University", new[] { "Inside The Walking Dead", "Inside The Walking Dead Walker University 2" })]
        [TestCase("Who.1999.S11E00.Twice.Upon.A.Time.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb", "Twice Upon A Time", new[] { "Last Christmas" })]
        [TestCase("Who.1999.S11E00.Twice.Upon.A.Time.Christmas.Special.720p.HDTV.x264-FoV", "Twice Upon A Time", new[] { "Last Christmas" })]
        [TestCase("Who.1999.S10E00.Christmas.Special.The.Return.Of.Doctor.Mysterio.1080p.BluRay.x264-OUIJA", "The Return Of Doctor Mysterio", new[] { "Doctor Mysterio" })]
        public void should_handle_special(string releaseTitle, string expectedTitle, string[] rejectedTitles)
        {
            GivenEpisodesWithTitles(rejectedTitles.Concat(new[] { expectedTitle }).ToArray());

            var episode = Subject.FindEpisodeByTitle(1, 0, releaseTitle);

            episode.Should().NotBeNull();
            episode.Title.Should().Be(expectedTitle);
        }

        [Test]
        public void should_handle_special_with_apostrophe_in_title()
        {
            var releaseTitle = "The.Profit.S06E00.An.Inside.Look-Sweet.Petes.720p.HDTV.";
            var title = "An Inside Look- Sweet Petes";

            GivenEpisodesWithTitles(title);

            var episode = Subject.FindEpisodeByTitle(1, 0, releaseTitle);

            episode.Should().NotBeNull();
            episode.Title.Should().Be(title);
        }
    }
}
