using System.Linq;
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
        private Episode _episode1;
        private Episode _episode2;

        [SetUp]
        public void Setup()
        {
            _episode1 = Builder<Episode>.CreateNew()
                                       .With(e => e.SeriesId = 1)
                                       .With(e => e.SeasonNumber = 1)
                                       .With(e => e.SceneSeasonNumber = 2)
                                       .With(e => e.EpisodeNumber = 3)
                                       .With(e => e.AbsoluteEpisodeNumber = 3)
                                       .With(e => e.SceneEpisodeNumber = 4)
                                       .BuildNew();

            _episode2 = Builder<Episode>.CreateNew()
                                        .With(e => e.SeriesId = 1)
                                        .With(e => e.SeasonNumber = 1)
                                        .With(e => e.SceneSeasonNumber = 2)
                                        .With(e => e.EpisodeNumber = 4)
                                        .With(e => e.SceneEpisodeNumber = 4)
                                        .BuildNew();

            _episode1 = Db.Insert(_episode1);
        }

        [Test]
        public void should_find_episode_by_scene_numbering()
        {
            Subject.FindEpisodesBySceneNumbering(_episode1.SeriesId, _episode1.SceneSeasonNumber.Value, _episode1.SceneEpisodeNumber.Value)
                   .First()
                   .Id
                   .Should()
                   .Be(_episode1.Id);
        }

        [Test]
        public void should_find_episode_by_standard_numbering()
        {
            Subject.Find(_episode1.SeriesId, _episode1.SeasonNumber, _episode1.EpisodeNumber)
                   .Id
                   .Should()
                   .Be(_episode1.Id);
        }

        [Test]
        public void should_not_find_episode_that_does_not_exist()
        {
            Subject.Find(_episode1.SeriesId, _episode1.SeasonNumber + 1, _episode1.EpisodeNumber)
                   .Should()
                   .BeNull();
        }

        [Test]
        public void should_find_episode_by_absolute_numbering()
        {
            Subject.Find(_episode1.SeriesId, _episode1.AbsoluteEpisodeNumber.Value)
                .Id
                .Should()
                .Be(_episode1.Id);
        }

        [Test]
        public void should_return_multiple_episode_if_multiple_match_by_scene_numbering()
        {
            _episode2 = Db.Insert(_episode2);

            Subject.FindEpisodesBySceneNumbering(_episode1.SeriesId, _episode1.SceneSeasonNumber.Value, _episode1.SceneEpisodeNumber.Value)
                   .Should()
                   .HaveCount(2);
        }
    }
}
