using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _episode = Builder<Episode>.CreateNew().Build();
        }

        private void GivenEpisodeTitle(string title)
        {
            _episode.Title = title;

            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(s => s.GetEpisodes(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(new List<Episode> { _episode });
        }

        [Test]
        public void should_find_episode_by_title()
        {
            GivenEpisodeTitle("A Journey to the Highlands");

            Subject.FindEpisodeByTitle(1, 1, "Downton.Abbey.A.Journey.To.The.Highlands.720p.BluRay.x264-aAF")
                   .Title
                   .Should()
                   .Be(_episode.Title);
        }
    }
}
