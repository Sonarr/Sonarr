using System.Threading.Tasks;
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
        public async Task should_delete_orphaned_episodes()
        {
            var episode = Builder<Episode>.CreateNew()
                                          .BuildNew();

            await Db.InsertAsync(episode);
            Subject.Clean();
            var episodes = await GetAllStoredModelsAsync();
            episodes.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_unorphaned_episodes()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            await Db.InsertAsync(series);

            var episodes = Builder<Episode>.CreateListOfSize(2)
                                          .TheFirst(1)
                                          .With(e => e.SeriesId = series.Id)
                                          .BuildListOfNew();

            await Db.InsertManyAsync(episodes);
            Subject.Clean();
            var loadedEpisode = await GetAllStoredModelsAsync();
            loadedEpisode.Should().HaveCount(1);
            loadedEpisode.Should().Contain(e => e.SeriesId == series.Id);
        }
    }
}
