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
        public void should_delete_orphaned_pending_items()
        {
            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.ParsedEpisodeInfo = new ParsedEpisodeInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_pending_items()
        {
            var series = Builder<Series>.CreateNew().BuildNew();

            Db.Insert(series);

            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.SeriesId = series.Id)
                .With(h => h.ParsedEpisodeInfo = new ParsedEpisodeInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
