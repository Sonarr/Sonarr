using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class FindEpisodeFixture : DbTest<EpisodeRepository, Episode>
    {
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _episode = Builder<Episode>.CreateNew()
                                          .With(e => e.Id = 0)
                                          .With(e => e.SeriesId = 1)
                                          .With(e => e.SeasonNumber = 1)
                                          .With(e => e.SceneSeasonNumber = 2)
                                          .With(e => e.EpisodeNumber = 3)
                                          .With(e => e.SceneEpisodeNumber = 4)
                                          .Build();

            _episode = Db.Insert(_episode);
        }

        [Test]
        public void should_find_episode_by_scene_numbering()
        {
            Subject.FindEpisodeBySceneNumbering(_episode.SeriesId, _episode.SceneSeasonNumber, _episode.SceneEpisodeNumber)
                   .Id
                   .Should()
                   .Be(_episode.Id);
        }

        [Test]
        public void should_find_episode_by_standard_numbering()
        {
            Subject.Find(_episode.SeriesId, _episode.SeasonNumber, _episode.EpisodeNumber)
                   .Id
                   .Should()
                   .Be(_episode.Id);
        }

        [Test]
        public void should_not_find_episode_that_does_not_exist()
        {
            Subject.Find(_episode.SeriesId, _episode.SeasonNumber + 1, _episode.EpisodeNumber)
                   .Should()
                   .BeNull();
        }
    }
}
