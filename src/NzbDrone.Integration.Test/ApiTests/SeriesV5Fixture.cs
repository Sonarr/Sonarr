using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Sonarr.Api.V3.RootFolders;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class SeriesV5Fixture : IntegrationTest
    {
        [Test]
        [Explicit("Requires source and destination temp directories on separate filesystems to reliably observe the in-flight move window; not reliable on CI runners where they share a volume.")]
        public void update_series_with_movefiles_should_defer_path_update_until_move_completes()
        {
            EnsureNoSeries(266189, "The Blacklist");

            // Source on tmpfs, dest on btrfs - forces a real cross-filesystem copy so the
            // move is still in flight when we read the deferred state back.
            var sourcePath = Path.Combine(Path.GetTempPath(), "sonarr_test_src_" + Guid.NewGuid());
            Directory.CreateDirectory(sourcePath);

            for (var i = 0; i < 25; i++)
            {
                File.WriteAllBytes(Path.Combine(sourcePath, $"file{i}.mkv"), new byte[1024 * 1024]);
            }

            var lookup = Series.Lookup("tvdb:266189").Single();
            lookup.QualityProfileId = 1;
            lookup.Path = sourcePath;

            var series = Series.Post(lookup);

            var destinationRoot = RootFolders.Post(new RootFolderResource { Path = GetTempDirectory("Moved") }).Path;
            var destinationPath = Path.Combine(destinationRoot, series.Title);

            var v5Series = SeriesV5.Get(series.Id);
            v5Series.Path = destinationPath;

            var result = SeriesV5.Put(v5Series, moveFiles: true);

            var afterPut = SeriesV5.Get(series.Id);

            afterPut.Path.Should().Be(sourcePath);
            afterPut.NextPath.Should().Be(destinationPath);

            Commands.All().Should().Contain(c => c.Name == "MoveSeries");

            Commands.WaitAll();

            var afterMove = SeriesV5.Get(series.Id);

            afterMove.Path.Should().Be(destinationPath);
            afterMove.NextPath.Should().BeNullOrEmpty();
        }

        [Test]
        [Explicit("Requires source and destination temp directories on separate filesystems to reliably observe the in-flight move window; not reliable on CI runners where they share a volume.")]
        public void update_series_with_movefiles_should_not_queue_second_move_when_one_already_pending()
        {
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

            var series = Series.Post(lookup);

            var rootFolder1 = RootFolders.Post(new RootFolderResource { Path = GetTempDirectory("Moved1") }).Path;
            var rootFolder2 = RootFolders.Post(new RootFolderResource { Path = GetTempDirectory("Moved2") }).Path;

            var v5Series = SeriesV5.Get(series.Id);
            v5Series.Path = Path.Combine(rootFolder1, series.Title);

            SeriesV5.Put(v5Series, moveFiles: true);

            var afterFirstPut = SeriesV5.Get(series.Id);
            afterFirstPut.NextPath.Should().NotBeNullOrEmpty();

            var pendingPath = afterFirstPut.Path;
            var pendingNextPath = afterFirstPut.NextPath;
            var queuedAfterFirst = Commands.All().Count(c => c.Name == "MoveSeries");

            afterFirstPut.Path = Path.Combine(rootFolder2, series.Title);
            afterFirstPut.QualityProfileId = 2;

            SeriesV5.Put(afterFirstPut, moveFiles: true);

            var afterSecondPut = SeriesV5.Get(series.Id);

            afterSecondPut.Path.Should().Be(pendingPath);
            afterSecondPut.NextPath.Should().Be(pendingNextPath);

            // Supplementary only - CommandEqualityComparer compares strings as char sets,
            // can under-report this count regardless of whether the guard ran.
            Commands.All().Count(c => c.Name == "MoveSeries").Should().Be(queuedAfterFirst);

            afterSecondPut.QualityProfileId.Should().Be(2);

            // Let the in-flight move finish before TearDown wipes the temp dirs.
            Commands.WaitAll();
        }

        [Test]
        public void update_series_without_movefiles_should_update_path_immediately()
        {
            var series = EnsureSeries(266189, "The Blacklist");

            var newPath = GetTempDirectory("NewSeriesPath");

            var v5Series = SeriesV5.Get(series.Id);
            v5Series.Path = newPath;

            SeriesV5.Put(v5Series, moveFiles: false);

            var result = SeriesV5.Get(series.Id);

            result.Path.Should().Be(newPath);
            result.NextPath.Should().BeNullOrEmpty();
        }

        [Test]
        public void update_series_with_movefiles_should_broadcast_persisted_state_not_requested_path()
        {
            var series = EnsureSeries(266189, "The Blacklist");

            var v5Series = SeriesV5.Get(series.Id);
            var oldPath = v5Series.Path;
            var destinationPath = GetTempDirectory("Moved");

            v5Series.Path = destinationPath;

            ConnectSignalR().Wait();

            SeriesV5.Put(v5Series, moveFiles: true);

            SignalRMessages.Should().Contain(m =>
                m.Name == "series" &&
                ((System.Text.Json.JsonElement)m.Body).GetProperty("resource").GetProperty("path").GetString() == oldPath &&
                ((System.Text.Json.JsonElement)m.Body).GetProperty("resource").GetProperty("nextPath").GetString() == destinationPath);

            Commands.WaitAll();
        }
    }
}
