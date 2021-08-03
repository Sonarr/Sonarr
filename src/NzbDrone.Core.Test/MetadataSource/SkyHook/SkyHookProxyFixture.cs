using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxyFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [TestCase(75978, "Family Guy")]
        [TestCase(83462, "Castle (2009)")]
        [TestCase(266189, "The Blacklist")]
        public void should_be_able_to_get_series_detail(int tvdbId, string title)
        {
            var details = Subject.GetSeriesInfo(tvdbId);

            ValidateSeries(details.Item1);
            ValidateEpisodes(details.Item2);

            details.Item1.Title.Should().Be(title);
        }

        [Test]
        public void getting_details_of_invalid_series()
        {
            Assert.Throws<SeriesNotFoundException>(() => Subject.GetSeriesInfo(int.MaxValue));
        }

        [Test]
        public void should_not_have_period_at_start_of_title_slug()
        {
            var details = Subject.GetSeriesInfo(79099);

            details.Item1.TitleSlug.Should().Be("dothack");
        }

        private void ValidateSeries(Series series)
        {
            series.Should().NotBeNull();
            series.Title.Should().NotBeNullOrWhiteSpace();
            series.CleanTitle.Should().Be(Parser.Parser.CleanSeriesTitle(series.Title));
            series.SortTitle.Should().Be(SeriesTitleNormalizer.Normalize(series.Title, series.TvdbId));
            series.Overview.Should().NotBeNullOrWhiteSpace();
            series.AirTime.Should().NotBeNullOrWhiteSpace();
            series.FirstAired.Should().HaveValue();
            series.FirstAired.Value.Kind.Should().Be(DateTimeKind.Utc);
            series.Images.Should().NotBeEmpty();
            series.ImdbId.Should().NotBeNullOrWhiteSpace();
            series.Network.Should().NotBeNullOrWhiteSpace();
            series.Runtime.Should().BeGreaterThan(0);
            series.TitleSlug.Should().NotBeNullOrWhiteSpace();

            //series.TvRageId.Should().BeGreaterThan(0);
            series.TvdbId.Should().BeGreaterThan(0);
        }

        private void ValidateEpisodes(List<Episode> episodes)
        {
            episodes.Should().NotBeEmpty();

            var episodeGroup = episodes.GroupBy(e => e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"));
            episodeGroup.Should().OnlyContain(c => c.Count() == 1);

            episodes.Should().Contain(c => c.SeasonNumber > 0);
            episodes.Should().Contain(c => !string.IsNullOrWhiteSpace(c.Overview));

            foreach (var episode in episodes)
            {
                ValidateEpisode(episode);

                //if atleast one episdoe has title it means parse it working.
                episodes.Should().Contain(c => !string.IsNullOrWhiteSpace(c.Title));
            }
        }

        private void ValidateEpisode(Episode episode)
        {
            episode.Should().NotBeNull();

            //TODO: Is there a better way to validate that episode number or season number is greater than zero?
            (episode.EpisodeNumber + episode.SeasonNumber).Should().NotBe(0);

            episode.Should().NotBeNull();

            if (episode.AirDateUtc.HasValue)
            {
                episode.AirDateUtc.Value.Kind.Should().Be(DateTimeKind.Utc);
            }

            episode.Images.Any(i => i.CoverType == MediaCoverTypes.Screenshot && i.Url.Contains("-940."))
                   .Should()
                   .BeFalse();
        }
    }
}
