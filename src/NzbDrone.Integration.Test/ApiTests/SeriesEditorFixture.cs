using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Sonarr.Api.V3.RootFolders;
using Sonarr.Api.V3.Series;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class SeriesEditorFixture : IntegrationTest
    {
        private void GivenExistingSeries()
        {
            WaitForCompletion(() => QualityProfiles.All().Count > 0);

            foreach (var title in new[] { "90210", "Dexter" })
            {
                var newSeries = Series.Lookup(title).First();

                newSeries.QualityProfileId = 1;
                newSeries.Path = string.Format(@"C:\Test\{0}", title).AsOsAgnostic();

                Series.Post(newSeries);
            }
        }

        [Test]
        public void should_be_able_to_update_multiple_series()
        {
            GivenExistingSeries();

            var series = Series.All();

            var seriesEditor = new SeriesEditorResource
            {
                QualityProfileId = 2,
                SeriesIds = series.Select(s => s.Id).ToList()
            };

            var result = Series.Editor(seriesEditor);

            result.Should().HaveCount(2);
            result.TrueForAll(s => s.QualityProfileId == 2).Should().BeTrue();
        }

        [Test]
        [Explicit("Requires source and destination temp directories on separate filesystems to reliably observe the in-flight move window; not reliable on CI runners where they share a volume.")]
        public void should_not_queue_a_second_move_for_a_series_with_a_move_already_pending()
        {
            // Uses its own series (distinct from GivenExistingSeries' "90210"/"Dexter", which
            // should_be_able_to_update_multiple_series also adds within the same shared app
            // instance) to avoid a "series already added" conflict when both tests run together.
            EnsureNoSeries(266189, "The Blacklist");

            // The source folder lives on a separate filesystem (tmpfs) from the root folders
            // below (which live under the btrfs-backed test output dir), and is seeded with real
            // files. This forces the first move to be a genuine cross-filesystem file copy rather
            // than an instant same-volume rename, so it is still in flight when the second request
            // arrives - without this, the move (and the NextPath clear that follows it) completes
            // before the second HTTP round-trip, and the guard can never be observed as pending.
            var sourcePath = Path.Combine(Path.GetTempPath(), "sonarr_test_src_" + Guid.NewGuid());
            Directory.CreateDirectory(sourcePath);

            for (var i = 0; i < 25; i++)
            {
                File.WriteAllBytes(Path.Combine(sourcePath, $"file{i}.mkv"), new byte[1024 * 1024]);
            }

            var lookup = Series.Lookup("tvdb:266189").Single();
            lookup.QualityProfileId = 1;
            lookup.Path = sourcePath;

            var target = Series.Post(lookup);

            var rootFolder1 = RootFolders.Post(new RootFolderResource { Path = GetTempDirectory("Moved1") }).Path;
            var rootFolder2 = RootFolders.Post(new RootFolderResource { Path = GetTempDirectory("Moved2") }).Path;

            var firstMove = Series.Editor(new SeriesEditorResource
            {
                SeriesIds = new List<int> { target.Id },
                RootFolderPath = rootFolder1,
                MoveFiles = true
            });

            firstMove.Single().Path.Should().NotBeNull();
            firstMove.Single().NextPath.Should().NotBeNullOrEmpty();

            var queuedAfterFirst = Commands.All().Count(c => c.Name == "BulkMoveSeries");

            var secondMove = Series.Editor(new SeriesEditorResource
            {
                SeriesIds = new List<int> { target.Id },
                QualityProfileId = 2,
                RootFolderPath = rootFolder2,
                MoveFiles = true
            });

            secondMove.Single().QualityProfileId.Should().Be(2);

            // The pending move must not have been disturbed by the second request: NextPath still
            // points at the first move's destination, proving the second request recognized a move
            // was already in flight for this series instead of recomputing/re-queuing one.
            secondMove.Single().NextPath.Should().Be(firstMove.Single().NextPath);

            // Supplementary signal only - the NextPath assertion above is the decisive,
            // non-confounded proof the guard worked. This count check is known-confoundable by a
            // separate bug in CommandEqualityComparer, which compares string properties (e.g.
            // DestinationRootFolder) as character sets via IEnumerable<char> rather than by value,
            // so it can under-report the true command count independent of whether the guard ran.
            Commands.All().Count(c => c.Name == "BulkMoveSeries").Should().Be(queuedAfterFirst);

            // Cleanup only: let the in-flight move finish before TearDown deletes the temp dirs it
            // is writing into. Must stay after all assertions above - the test's signal depends on
            // the second request racing the still-running first move.
            Commands.WaitAll();
        }
    }
}
