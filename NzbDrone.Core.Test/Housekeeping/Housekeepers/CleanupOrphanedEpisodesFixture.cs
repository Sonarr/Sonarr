using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedEpisodesFixture : DbTest<CleanupOrphanedEpisodes, Episode>
    {
        [Test]
        public void should_delete_orphaned_episodes()
        {
            var episode = Builder<Episode>.CreateNew()
                                          .BuildNew();

            Db.Insert(episode);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_episodes()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            Db.Insert(series);

            var episodes = Builder<Episode>.CreateListOfSize(2)
                                          .TheFirst(1)
                                          .With(e => e.SeriesId = series.Id)
                                          .BuildListOfNew();

            Db.InsertMany(episodes);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(e => e.SeriesId == series.Id);
        }
    }
}
