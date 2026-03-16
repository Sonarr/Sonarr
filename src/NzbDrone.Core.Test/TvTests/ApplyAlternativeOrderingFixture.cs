using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.Tvdb;
using NzbDrone.Core.MetadataSource.Tvdb.Resource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class ApplyAlternativeOrderingFixture : CoreTest<RefreshSeriesService>
    {
        private Series _series;
        private List<Episode> _episodes;
        private List<TvdbEpisodeResource> _tvdbDvdEpisodes;

        [SetUp]
        public void Setup()
        {
            _series = new Series
            {
                Id = 1,
                TvdbId = 78874,
                Title = "Firefly",
                Status = SeriesStatusType.Ended,
                EpisodeOrder = EpisodeOrderType.Default,
                Seasons = new List<Season>
                {
                    new Season { SeasonNumber = 1, Monitored = true }
                }
            };

            _episodes = new List<Episode>
            {
                new Episode { TvdbId = 297999, SeasonNumber = 1, EpisodeNumber = 1, Title = "The Train Job" },
                new Episode { TvdbId = 297989, SeasonNumber = 1, EpisodeNumber = 2, Title = "Bushwhacked" },
                new Episode { TvdbId = 297990, SeasonNumber = 1, EpisodeNumber = 3, Title = "Shindig" },
            };

            // DVD ordering remaps S/E numbers while keeping the same TVDB IDs
            _tvdbDvdEpisodes = new List<TvdbEpisodeResource>
            {
                new TvdbEpisodeResource { Id = 297999, SeasonNumber = 1, Number = 2, Name = "The Train Job (DVD pos 2)" },
                new TvdbEpisodeResource { Id = 297989, SeasonNumber = 1, Number = 3, Name = "Bushwhacked (DVD pos 3)" },
                new TvdbEpisodeResource { Id = 297990, SeasonNumber = 1, Number = 1, Name = "Shindig (DVD pos 1)" },
            };

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.TvdbApiKey)
                .Returns("test-api-key");

            Mocker.GetMock<ITvdbApiClient>()
                .Setup(s => s.GetEpisodesByOrdering(_series.TvdbId, It.IsAny<EpisodeOrderType>(), It.IsAny<string>()))
                .Returns(_tvdbDvdEpisodes);
        }

        private List<Episode> InvokeApplyAlternativeOrdering()
        {
            // ApplyAlternativeOrdering is private, but it's called from RefreshSeriesInfo
            // which requires extensive setup. Instead, use reflection to test it directly.
            var method = typeof(RefreshSeriesService).GetMethod(
                "ApplyAlternativeOrdering",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            return (List<Episode>)method.Invoke(Subject, new object[] { _series, _episodes });
        }

        [Test]
        public void should_return_unchanged_when_default_ordering_and_no_overrides()
        {
            _series.EpisodeOrder = EpisodeOrderType.Default;
            _series.Seasons.ForEach(s => s.EpisodeOrderOverride = null);

            var result = InvokeApplyAlternativeOrdering();

            result.Should().BeSameAs(_episodes);

            Mocker.GetMock<ITvdbApiClient>()
                .Verify(v => v.GetEpisodesByOrdering(It.IsAny<int>(), It.IsAny<EpisodeOrderType>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_unchanged_when_no_tvdb_api_key()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.TvdbApiKey)
                .Returns(string.Empty);

            var result = InvokeApplyAlternativeOrdering();

            result.Should().BeSameAs(_episodes);

            Mocker.GetMock<ITvdbApiClient>()
                .Verify(v => v.GetEpisodesByOrdering(It.IsAny<int>(), It.IsAny<EpisodeOrderType>(), It.IsAny<string>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_remap_episodes_with_series_level_dvd_ordering()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            var result = InvokeApplyAlternativeOrdering();

            // Episode 297999 should move from E1 to E2
            var ep1 = result.Single(e => e.TvdbId == 297999);
            ep1.EpisodeNumber.Should().Be(2);

            // Episode 297989 should move from E2 to E3
            var ep2 = result.Single(e => e.TvdbId == 297989);
            ep2.EpisodeNumber.Should().Be(3);

            // Episode 297990 should move from E3 to E1
            var ep3 = result.Single(e => e.TvdbId == 297990);
            ep3.EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_remap_using_season_override()
        {
            // Series-level is default, but Season 1 overrides to DVD
            _series.EpisodeOrder = EpisodeOrderType.Default;
            _series.Seasons[0].EpisodeOrderOverride = EpisodeOrderType.Dvd;

            var result = InvokeApplyAlternativeOrdering();

            // Should still remap because of the season override
            var ep = result.Single(e => e.TvdbId == 297990);
            ep.EpisodeNumber.Should().Be(1);

            Mocker.GetMock<ITvdbApiClient>()
                .Verify(v => v.GetEpisodesByOrdering(_series.TvdbId, EpisodeOrderType.Dvd, It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_only_remap_seasons_with_matching_ordering()
        {
            _series.EpisodeOrder = EpisodeOrderType.Default;
            _series.Seasons = new List<Season>
            {
                new Season { SeasonNumber = 1, Monitored = true, EpisodeOrderOverride = EpisodeOrderType.Dvd },
                new Season { SeasonNumber = 2, Monitored = true }  // No override — keep default
            };

            // Add a season 2 episode
            _episodes.Add(new Episode { TvdbId = 400, SeasonNumber = 2, EpisodeNumber = 1, Title = "S2 Ep1" });

            var result = InvokeApplyAlternativeOrdering();

            // Season 1 episodes should be remapped
            result.Single(e => e.TvdbId == 297990).EpisodeNumber.Should().Be(1);

            // Season 2 episode should NOT be remapped (no override)
            var s2ep = result.Single(e => e.TvdbId == 400);
            s2ep.SeasonNumber.Should().Be(2);
            s2ep.EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_skip_episodes_with_zero_tvdb_id()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            // Add an episode with TvdbId 0 (can't be matched)
            _episodes.Add(new Episode { TvdbId = 0, SeasonNumber = 1, EpisodeNumber = 4, Title = "Unknown" });

            var result = InvokeApplyAlternativeOrdering();

            // The zero-TvdbId episode should keep its original numbering
            var unknown = result.Single(e => e.TvdbId == 0);
            unknown.EpisodeNumber.Should().Be(4);
        }

        [Test]
        public void should_keep_original_numbering_when_tvdb_id_not_in_lookup()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            // Add an episode whose TvdbId is NOT in the TVDB DVD response
            _episodes.Add(new Episode { TvdbId = 999999, SeasonNumber = 1, EpisodeNumber = 4, Title = "Missing from DVD" });

            var result = InvokeApplyAlternativeOrdering();

            // Should keep original numbering since TVDB doesn't have it
            var missing = result.Single(e => e.TvdbId == 999999);
            missing.EpisodeNumber.Should().Be(4);
            missing.SeasonNumber.Should().Be(1);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_update_absolute_number_when_available()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            // TVDB response includes AbsoluteNumber for one episode
            _tvdbDvdEpisodes[0].AbsoluteNumber = 42;

            var result = InvokeApplyAlternativeOrdering();

            var ep = result.Single(e => e.TvdbId == 297999);
            ep.AbsoluteEpisodeNumber.Should().Be(42);
        }

        [Test]
        public void should_not_overwrite_absolute_number_when_null_in_tvdb()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            // Episode starts with an absolute number, TVDB has null
            _episodes[0].AbsoluteEpisodeNumber = 10;
            _tvdbDvdEpisodes[0].AbsoluteNumber = null;

            var result = InvokeApplyAlternativeOrdering();

            // Should NOT overwrite because the TVDB value has no AbsoluteNumber
            var ep = result.Single(e => e.TvdbId == 297999);
            ep.AbsoluteEpisodeNumber.Should().Be(10);
        }

        [Test]
        public void should_remap_season_numbers()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            // TVDB DVD ordering puts an episode into a different season
            _tvdbDvdEpisodes[2].SeasonNumber = 2;
            _tvdbDvdEpisodes[2].Number = 1;

            var result = InvokeApplyAlternativeOrdering();

            var ep = result.Single(e => e.TvdbId == 297990);
            ep.SeasonNumber.Should().Be(2);
            ep.EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_handle_tvdb_api_failure_gracefully()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            Mocker.GetMock<ITvdbApiClient>()
                .Setup(s => s.GetEpisodesByOrdering(It.IsAny<int>(), It.IsAny<EpisodeOrderType>(), It.IsAny<string>()))
                .Throws(new Exception("TVDB API timeout"));

            var result = InvokeApplyAlternativeOrdering();

            // Should return episodes unchanged, not throw
            result.Should().HaveCount(3);
            result.Single(e => e.TvdbId == 297999).EpisodeNumber.Should().Be(1, "original numbering preserved on API failure");

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_handle_empty_tvdb_response()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            Mocker.GetMock<ITvdbApiClient>()
                .Setup(s => s.GetEpisodesByOrdering(It.IsAny<int>(), It.IsAny<EpisodeOrderType>(), It.IsAny<string>()))
                .Returns(new List<TvdbEpisodeResource>());

            var result = InvokeApplyAlternativeOrdering();

            // Should return episodes unchanged
            result.Should().HaveCount(3);
            result.Single(e => e.TvdbId == 297999).EpisodeNumber.Should().Be(1);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_handle_null_tvdb_response()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            Mocker.GetMock<ITvdbApiClient>()
                .Setup(s => s.GetEpisodesByOrdering(It.IsAny<int>(), It.IsAny<EpisodeOrderType>(), It.IsAny<string>()))
                .Returns((List<TvdbEpisodeResource>)null);

            var result = InvokeApplyAlternativeOrdering();

            result.Should().HaveCount(3);
            result.Single(e => e.TvdbId == 297999).EpisodeNumber.Should().Be(1);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_handle_multiple_ordering_types_across_seasons()
        {
            _series.EpisodeOrder = EpisodeOrderType.Default;
            _series.Seasons = new List<Season>
            {
                new Season { SeasonNumber = 1, Monitored = true, EpisodeOrderOverride = EpisodeOrderType.Dvd },
                new Season { SeasonNumber = 2, Monitored = true, EpisodeOrderOverride = EpisodeOrderType.Absolute },
            };

            // Season 2 episode
            _episodes.Add(new Episode { TvdbId = 500, SeasonNumber = 2, EpisodeNumber = 1, Title = "S2 Ep1" });

            var absoluteEpisodes = new List<TvdbEpisodeResource>
            {
                new TvdbEpisodeResource { Id = 500, SeasonNumber = 1, Number = 15, AbsoluteNumber = 15, Name = "S2 Ep1 Absolute" },
            };

            // DVD ordering returns for Season 1
            Mocker.GetMock<ITvdbApiClient>()
                .Setup(s => s.GetEpisodesByOrdering(_series.TvdbId, EpisodeOrderType.Dvd, It.IsAny<string>()))
                .Returns(_tvdbDvdEpisodes);

            // Absolute ordering returns for Season 2
            Mocker.GetMock<ITvdbApiClient>()
                .Setup(s => s.GetEpisodesByOrdering(_series.TvdbId, EpisodeOrderType.Absolute, It.IsAny<string>()))
                .Returns(absoluteEpisodes);

            var result = InvokeApplyAlternativeOrdering();

            // Season 1 uses DVD
            result.Single(e => e.TvdbId == 297990).EpisodeNumber.Should().Be(1);

            // Season 2 uses Absolute
            var s2ep = result.Single(e => e.TvdbId == 500);
            s2ep.SeasonNumber.Should().Be(1);
            s2ep.EpisodeNumber.Should().Be(15);
            s2ep.AbsoluteEpisodeNumber.Should().Be(15);
        }

        [Test]
        public void should_skip_tvdb_episodes_with_null_season_or_number()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;

            // TVDB response has an episode with null season/number
            _tvdbDvdEpisodes.Add(new TvdbEpisodeResource { Id = 888, SeasonNumber = null, Number = null, Name = "Bad Data" });

            var result = InvokeApplyAlternativeOrdering();

            // Should still remap the valid ones without error
            result.Single(e => e.TvdbId == 297990).EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_return_unchanged_when_all_season_overrides_are_default()
        {
            // Series-level ordering is non-default, but all season overrides point to default
            _series.EpisodeOrder = EpisodeOrderType.Default;
            _series.Seasons[0].EpisodeOrderOverride = null;

            var result = InvokeApplyAlternativeOrdering();

            result.Should().BeSameAs(_episodes);
        }

        [Test]
        public void should_call_tvdb_once_per_ordering_type()
        {
            _series.EpisodeOrder = EpisodeOrderType.Dvd;
            _series.Seasons = new List<Season>
            {
                new Season { SeasonNumber = 1, Monitored = true },
                new Season { SeasonNumber = 2, Monitored = true },
                new Season { SeasonNumber = 3, Monitored = true },
            };

            InvokeApplyAlternativeOrdering();

            // All three seasons use the same ordering (DVD from series-level), so only one API call
            Mocker.GetMock<ITvdbApiClient>()
                .Verify(v => v.GetEpisodesByOrdering(_series.TvdbId, EpisodeOrderType.Dvd, It.IsAny<string>()), Times.Once());
        }
    }
}
