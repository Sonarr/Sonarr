using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class CleanupOrphanedEpisodesFixture : DbTest<EpisodeRepository, Episode>
    {
        [Test]
        public void should_delete_orphaned_episodes()
        {
            var episode = Builder<Episode>.CreateNew()
                                          .BuildNew();

            Subject.Insert(episode);
            Subject.CleanupOrphanedEpisodes();
            Subject.All().Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_episodes()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            Db.Insert(series);

            var episodes = Builder<Episode>.CreateListOfSize(2)
                                          .All()
                                          .With(e => e.Id = 0)
                                          .TheFirst(1)
                                          .With(e => e.SeriesId = series.Id)
                                          .Build();

            Subject.InsertMany(episodes);
            Subject.CleanupOrphanedEpisodes();
            Subject.All().Should().HaveCount(1);
            Subject.All().Should().Contain(e => e.SeriesId == series.Id);
        }
    }
}
