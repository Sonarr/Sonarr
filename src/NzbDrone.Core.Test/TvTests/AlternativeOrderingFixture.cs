using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class AlternativeOrderingFixture : CoreTest<RefreshEpisodeService>
    {
        private List<Episode> _insertedEpisodes;
        private List<Episode> _updatedEpisodes;
        private List<Episode> _deletedEpisodes;

        [SetUp]
        public void Setup()
        {
            _insertedEpisodes = new List<Episode>();
            _updatedEpisodes = new List<Episode>();
            _deletedEpisodes = new List<Episode>();

            Mocker.GetMock<IEpisodeService>().Setup(c => c.InsertMany(It.IsAny<List<Episode>>()))
                .Callback<List<Episode>>(e => _insertedEpisodes = e);

            Mocker.GetMock<IEpisodeService>().Setup(c => c.UpdateMany(It.IsAny<List<Episode>>()))
                .Callback<List<Episode>>(e => _updatedEpisodes = e);

            Mocker.GetMock<IEpisodeService>().Setup(c => c.DeleteMany(It.IsAny<List<Episode>>()))
                .Callback<List<Episode>>(e => _deletedEpisodes = e);
        }

        private Series GetSeries(EpisodeOrderType orderType = EpisodeOrderType.Default)
        {
            return Builder<Series>.CreateNew()
                .With(s => s.SeriesType = SeriesTypes.Standard)
                .With(s => s.Status = SeriesStatusType.Ended)
                .With(s => s.EpisodeOrder = orderType)
                .With(s => s.Seasons = new List<Season>
                {
                    new Season { SeasonNumber = 1, Monitored = true }
                })
                .Build();
        }

        private List<Episode> GetExistingEpisodes()
        {
            return new List<Episode>
            {
                new Episode { Id = 1, SeriesId = 1, TvdbId = 100, SeasonNumber = 1, EpisodeNumber = 1, Title = "Ep1", EpisodeFileId = 10 },
                new Episode { Id = 2, SeriesId = 1, TvdbId = 101, SeasonNumber = 1, EpisodeNumber = 2, Title = "Ep2", EpisodeFileId = 11 },
                new Episode { Id = 3, SeriesId = 1, TvdbId = 102, SeasonNumber = 1, EpisodeNumber = 3, Title = "Ep3", EpisodeFileId = 12 },
            };
        }

        private List<Episode> GetRemoteEpisodesWithDvdOrder()
        {
            // In DVD order, episodes are reordered — same TvdbIds, different S/E numbers
            return new List<Episode>
            {
                new Episode { TvdbId = 102, SeasonNumber = 1, EpisodeNumber = 1, Title = "Ep3 (DVD first)" },
                new Episode { TvdbId = 100, SeasonNumber = 1, EpisodeNumber = 2, Title = "Ep1 (DVD second)" },
                new Episode { TvdbId = 101, SeasonNumber = 1, EpisodeNumber = 3, Title = "Ep2 (DVD third)" },
            };
        }

        [Test]
        public void should_match_by_tvdb_id_when_alternative_ordering_active()
        {
            var series = GetSeries(EpisodeOrderType.Dvd);
            var existing = GetExistingEpisodes();
            var remote = GetRemoteEpisodesWithDvdOrder();

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existing);

            Subject.RefreshEpisodeInfo(series, remote);

            // All 3 should be updated (matched by TvdbId), none inserted or deleted
            _updatedEpisodes.Should().HaveCount(3);
            _insertedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();

            // Episode with TvdbId 100 (was E1) should now be E2 (DVD order)
            var ep100 = _updatedEpisodes.Single(e => e.TvdbId == 100);
            ep100.EpisodeNumber.Should().Be(2);
            ep100.EpisodeFileId.Should().Be(10, "file association should be preserved");

            // Episode with TvdbId 102 (was E3) should now be E1 (DVD order)
            var ep102 = _updatedEpisodes.Single(e => e.TvdbId == 102);
            ep102.EpisodeNumber.Should().Be(1);
            ep102.EpisodeFileId.Should().Be(12, "file association should be preserved");
        }

        [Test]
        public void should_match_by_season_episode_when_default_ordering()
        {
            var series = GetSeries(EpisodeOrderType.Default);
            var existing = GetExistingEpisodes();

            // Remote episodes with same S/E but different TvdbIds (simulating standard match)
            var remote = new List<Episode>
            {
                new Episode { TvdbId = 100, SeasonNumber = 1, EpisodeNumber = 1, Title = "Ep1 Updated" },
                new Episode { TvdbId = 101, SeasonNumber = 1, EpisodeNumber = 2, Title = "Ep2 Updated" },
                new Episode { TvdbId = 102, SeasonNumber = 1, EpisodeNumber = 3, Title = "Ep3 Updated" },
            };

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existing);

            Subject.RefreshEpisodeInfo(series, remote);

            _updatedEpisodes.Should().HaveCount(3);
            _insertedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_fallback_to_season_episode_matching_when_tvdb_id_not_found()
        {
            var series = GetSeries(EpisodeOrderType.Dvd);

            // Existing episode with TvdbId = 0 (missing TvdbId)
            var existing = new List<Episode>
            {
                new Episode { Id = 1, SeriesId = 1, TvdbId = 0, SeasonNumber = 1, EpisodeNumber = 1, Title = "Ep1", EpisodeFileId = 10 },
            };

            // Remote episode with TvdbId that won't match, but S/E will match
            var remote = new List<Episode>
            {
                new Episode { TvdbId = 999, SeasonNumber = 1, EpisodeNumber = 1, Title = "Ep1 DVD" },
            };

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existing);

            Subject.RefreshEpisodeInfo(series, remote);

            // Should still match by S/E as fallback
            _updatedEpisodes.Should().HaveCount(1);
            _insertedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_preserve_file_associations_when_switching_ordering()
        {
            var series = GetSeries(EpisodeOrderType.Dvd);
            var existing = GetExistingEpisodes();
            var remote = GetRemoteEpisodesWithDvdOrder();

            // Snapshot original file associations before RefreshEpisodeInfo mutates the list
            var originalFileIds = existing.ToDictionary(e => e.TvdbId, e => e.EpisodeFileId);

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existing);

            Subject.RefreshEpisodeInfo(series, remote);

            // Every updated episode should retain its original EpisodeFileId
            foreach (var updated in _updatedEpisodes)
            {
                originalFileIds.Should().ContainKey(updated.TvdbId);
                updated.EpisodeFileId.Should().Be(originalFileIds[updated.TvdbId],
                    $"EpisodeFileId for TvdbId {updated.TvdbId} should be preserved across ordering change");
            }
        }

        [Test]
        public void should_detect_alternative_ordering_from_season_override()
        {
            var series = GetSeries(EpisodeOrderType.Default);
            series.Seasons = new List<Season>
            {
                new Season { SeasonNumber = 1, Monitored = true, EpisodeOrderOverride = EpisodeOrderType.Dvd }
            };

            var existing = GetExistingEpisodes();
            var remote = GetRemoteEpisodesWithDvdOrder();

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existing);

            Subject.RefreshEpisodeInfo(series, remote);

            // Should use TvdbId matching because of per-season override
            _updatedEpisodes.Should().HaveCount(3);

            var ep102 = _updatedEpisodes.Single(e => e.TvdbId == 102);
            ep102.EpisodeNumber.Should().Be(1, "DVD ordering should be applied via season override");
        }

        [Test]
        public void should_handle_new_episodes_in_alternative_ordering()
        {
            var series = GetSeries(EpisodeOrderType.Dvd);

            var existing = new List<Episode>
            {
                new Episode { Id = 1, SeriesId = 1, TvdbId = 100, SeasonNumber = 1, EpisodeNumber = 1, Title = "Ep1" },
            };

            // Remote has existing episode + a new one
            // Episodes are processed in S/E order, so S1E1 (TvdbId=200) is matched first.
            // TvdbId=200 has no TvdbId match, but falls back to S/E match (S1E1) against the existing episode.
            // Then S1E2 (TvdbId=100) has no remaining existing episode to match, so it gets inserted.
            var remote = new List<Episode>
            {
                new Episode { TvdbId = 100, SeasonNumber = 1, EpisodeNumber = 2, Title = "Ep1 Moved" },
                new Episode { TvdbId = 200, SeasonNumber = 1, EpisodeNumber = 1, Title = "New DVD Ep1" },
            };

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existing);

            Subject.RefreshEpisodeInfo(series, remote);

            // S1E1 (TvdbId=200) matched existing episode via S/E fallback — updated with new TvdbId
            _updatedEpisodes.Should().HaveCount(1);
            _updatedEpisodes[0].TvdbId.Should().Be(200);
            _updatedEpisodes[0].EpisodeNumber.Should().Be(1);

            // S1E2 (TvdbId=100) had no match — inserted as new
            _insertedEpisodes.Should().HaveCount(1);
            _insertedEpisodes[0].TvdbId.Should().Be(100);
        }
    }
}
