using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedPendingReleasesFixture : DbTest<CleanupOrphanedPendingReleases, PendingRelease>
    {
        [Test]
        public async Task should_delete_orphaned_pending_items()
        {
            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.ParsedEpisodeInfo = new ParsedEpisodeInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            await Db.InsertAsync(pendingRelease);
            Subject.Clean();
            var pendingReleases = await GetAllStoredModelsAsync();
            pendingReleases.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_unorphaned_pending_items()
        {
            var series = Builder<Series>.CreateNew().BuildNew();

            await Db.InsertAsync(series);

            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.SeriesId = series.Id)
                .With(h => h.ParsedEpisodeInfo = new ParsedEpisodeInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            await Db.InsertAsync(pendingRelease);

            Subject.Clean();
            var pendingReleases = await GetAllStoredModelsAsync();
            pendingReleases.Should().HaveCount(1);
        }
    }
}
