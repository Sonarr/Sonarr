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
            // Own series (not 90210/Dexter) to avoid clashing with the other test.
            EnsureNoSeries(266189, "The Blacklist");

            // Source on tmpfs, dest on btrfs - forces a real cross-filesystem copy so the
            // first move is still in flight when the second request lands.
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

            // NextPath unchanged - second request recognized a move was already pending,
            // didn't re-queue one.
            secondMove.Single().NextPath.Should().Be(firstMove.Single().NextPath);

            // Supplementary only - CommandEqualityComparer compares strings as char sets,
            // can under-report this count regardless of whether the guard ran.
            Commands.All().Count(c => c.Name == "BulkMoveSeries").Should().Be(queuedAfterFirst);

            // Let the in-flight move finish before TearDown wipes the temp dirs.
            Commands.WaitAll();
        }
    }
}
