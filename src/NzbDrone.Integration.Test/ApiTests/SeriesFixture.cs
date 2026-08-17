using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Sonarr.Api.V3.RootFolders;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class SeriesFixture : IntegrationTest
    {
        [Test]
        [Order(0)]
        public void add_series_with_tags_should_store_them()
        {
            EnsureNoSeries(266189, "The Blacklist");
            var tag = EnsureTag("abc");

            var series = Series.Lookup("tvdb:266189").Single();

            series.QualityProfileId = 1;
            series.Path = Path.Combine(SeriesRootFolder, series.Title);
            series.Tags = new HashSet<int>();
            series.Tags.Add(tag.Id);

            var result = Series.Post(series);

            result.Should().NotBeNull();
            result.Tags.Should().Equal(tag.Id);
        }

        [Test]
        [Order(0)]
        public void add_series_without_profileid_should_return_badrequest()
        {
            EnsureNoSeries(266189, "The Blacklist");

            var series = Series.Lookup("tvdb:266189").Single();

            series.Path = Path.Combine(SeriesRootFolder, series.Title);

            Series.InvalidPost(series);
        }

        [Test]
        [Order(0)]
        public void add_series_without_path_should_return_badrequest()
        {
            EnsureNoSeries(266189, "The Blacklist");

            var series = Series.Lookup("tvdb:266189").Single();

            series.QualityProfileId = 1;

            Series.InvalidPost(series);
        }

        [Test]
        [Order(1)]
        public void add_series()
        {
            EnsureNoSeries(266189, "The Blacklist");

            var series = Series.Lookup("tvdb:266189").Single();

            series.QualityProfileId = 1;
            series.Path = Path.Combine(SeriesRootFolder, series.Title);

            var result = Series.Post(series);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);
            result.QualityProfileId.Should().Be(1);
            result.Path.Should().Be(Path.Combine(SeriesRootFolder, series.Title));
        }

        [Test]
        [Order(2)]
        public void get_all_series()
        {
            EnsureSeries(266189, "The Blacklist");
            EnsureSeries(73065, "Archer");

            Series.All().Should().NotBeNullOrEmpty();
            Series.All().Should().Contain(v => v.TvdbId == 73065);
            Series.All().Should().Contain(v => v.TvdbId == 266189);
        }

        [Test]
        [Order(2)]
        public void get_series_by_id()
        {
            var series = EnsureSeries(266189, "The Blacklist");

            var result = Series.Get(series.Id);

            result.TvdbId.Should().Be(266189);
        }

        [Test]
        public void get_series_by_unknown_id_should_return_404()
        {
            var result = Series.InvalidGet(1000000);
        }

        [Test]
        [Order(2)]
        public void update_series_profile_id()
        {
            var series = EnsureSeries(266189, "The Blacklist");

            var profileId = 1;
            if (series.QualityProfileId == profileId)
            {
                profileId = 2;
            }

            series.QualityProfileId = profileId;

            var result = Series.Put(series);

            Series.Get(series.Id).QualityProfileId.Should().Be(profileId);
        }

        [Test]
        [Order(3)]
        public void update_series_monitored()
        {
            var series = EnsureSeries(266189, "The Blacklist", false);

            series.Monitored.Should().BeFalse();
            series.Seasons.First().Monitored.Should().BeFalse();

            series.Monitored = true;
            series.Seasons.ForEach(season =>
            {
                season.Monitored = true;
            });

            var result = Series.Put(series);

            result.Monitored.Should().BeTrue();
            result.Seasons.First().Monitored.Should().BeTrue();
        }

        [Test]
        [Order(3)]
        public void update_series_tags()
        {
            var series = EnsureSeries(266189, "The Blacklist");
            var tag = EnsureTag("abc");

            if (series.Tags.Contains(tag.Id))
            {
                series.Tags.Remove(tag.Id);

                var result = Series.Put(series);
                Series.Get(series.Id).Tags.Should().NotContain(tag.Id);
            }
            else
            {
                series.Tags.Add(tag.Id);

                var result = Series.Put(series);
                Series.Get(series.Id).Tags.Should().Contain(tag.Id);
            }
        }

        [Test]
        [Order(4)]
        public void delete_series()
        {
            var series = EnsureSeries(266189, "The Blacklist");

            Series.Get(series.Id).Should().NotBeNull();

            Series.Delete(series.Id);

            Series.All().Should().NotContain(v => v.TvdbId == 266189);
        }

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

            series.Path = destinationPath;

            var result = Series.Put(series, moveFiles: true);

            var afterPut = Series.Get(series.Id);

            afterPut.Path.Should().Be(sourcePath);
            afterPut.NextPath.Should().Be(destinationPath);

            Commands.All().Should().Contain(c => c.Name == "MoveSeries");

            Commands.WaitAll();

            var afterMove = Series.Get(series.Id);

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

            series.Path = Path.Combine(rootFolder1, series.Title);

            Series.Put(series, moveFiles: true);

            var afterFirstPut = Series.Get(series.Id);
            afterFirstPut.NextPath.Should().NotBeNullOrEmpty();

            var queuedAfterFirst = Commands.All().Count(c => c.Name == "MoveSeries");

            series.Path = Path.Combine(rootFolder2, series.Title);
            series.QualityProfileId = 2;

            Series.Put(series, moveFiles: true);

            var afterSecondPut = Series.Get(series.Id);

            afterSecondPut.Path.Should().Be(afterFirstPut.Path);
            afterSecondPut.NextPath.Should().Be(afterFirstPut.NextPath);

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
            series.Path = newPath;

            Series.Put(series, moveFiles: false);

            var result = Series.Get(series.Id);

            result.Path.Should().Be(newPath);
            result.NextPath.Should().BeNullOrEmpty();
        }

        [Test]
        public void update_series_with_movefiles_should_broadcast_persisted_state_not_requested_path()
        {
            var series = EnsureSeries(266189, "The Blacklist");

            var oldPath = series.Path;
            var destinationPath = GetTempDirectory("Moved");

            series.Path = destinationPath;

            ConnectSignalR().Wait();

            Series.Put(series, moveFiles: true);

            SignalRMessages.Should().Contain(m =>
                m.Name == "series" &&
                ((System.Text.Json.JsonElement)m.Body).GetProperty("resource").GetProperty("path").GetString() == oldPath &&
                ((System.Text.Json.JsonElement)m.Body).GetProperty("resource").GetProperty("nextPath").GetString() == destinationPath);

            Commands.WaitAll();
        }
    }
}
