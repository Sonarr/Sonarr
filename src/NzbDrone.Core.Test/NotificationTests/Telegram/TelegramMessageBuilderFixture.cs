using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Notifications.Telegram;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests.Telegram
{
    [TestFixture]
    public partial class TelegramMessageBuilderFixture : TestBase
    {
        private const string Screenshot = "https://artworks.thetvdb.com/screencap.jpg";
        private const string Fanart = "https://artworks.thetvdb.com/fanart.jpg";
        private const string Poster = "https://artworks.thetvdb.com/poster.jpg";

        private TelegramSettings _settings;
        private Series _series;
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _settings = new TelegramSettings();

            _series = new Series
            {
                Title = "Lanterns",
                Year = 2026,
                Network = "HBO",
                SeriesType = SeriesTypes.Standard,
                Ratings = new Ratings { Votes = 84576, Value = 7.8m },
                Images =
                [
                    new MediaCover.MediaCover(MediaCoverTypes.Fanart, Fanart),
                    new MediaCover.MediaCover(MediaCoverTypes.Poster, Poster)
                ]
            };

            _episode = new Episode
            {
                SeasonNumber = 1,
                EpisodeNumber = 1,
                Title = "Pilot",
                AirDate = "2026-08-16",
                Overview = "After a fatal shooting, Green Lantern Hal Jordan and new recruit John Stewart set out to prove it was an extraterrestrial event.",
                Images =
                [
                    new MediaCover.MediaCover(MediaCoverTypes.Screenshot, Screenshot)
                ]
            };
        }

        [Test]
        public void ShouldBuildTheAgreedCardLayout()
        {
            var message = Build();

            message.Text.Should().Be(
                "<b>Lanterns · S01E01</b>\n" +
                "<i>Pilot</i>\n" +
                "\n" +
                "<blockquote expandable>After a fatal shooting, Green Lantern Hal Jordan and new recruit John Stewart set out to prove it was an extraterrestrial event.</blockquote>\n" +
                "\n" +
                "7.8/10 · 16 Aug 2026\n" +
                "<i>WEBDL-1080p Proper · 4.4 GB</i>\n" +
                "<i>Trigger: Episode Downloaded</i>");
        }

        [Test]
        public void ShouldPreferTheEpisodeScreenshot()
        {
            Build().Photo.Should().Be(Screenshot);
        }

        [Test]
        public void ShouldFallBackToFanartWhenThereIsNoScreenshot()
        {
            _episode.Images.Clear();

            Build().Photo.Should().Be(Fanart);
        }

        [Test]
        public void ShouldFallBackToPosterWhenThereIsNoScreenshotOrFanart()
        {
            _episode.Images.Clear();
            _series.Images.RemoveAll(i => i.CoverType == MediaCoverTypes.Fanart);

            Build().Photo.Should().Be(Poster);
        }

        [Test]
        public void ShouldSendWithoutAPhotoWhenNoImageIsAvailable()
        {
            _episode.Images.Clear();
            _series.Images.Clear();

            var message = Build();

            message.HasPhoto.Should().BeFalse();
            message.Text.Should().Contain("<b>Lanterns · S01E01</b>");
        }

        [Test]
        public void ShouldSendWithoutAPhotoWhenImagesAreDisabled()
        {
            _settings.Image = (int)TelegramImageType.None;

            Build().HasPhoto.Should().BeFalse();
        }

        [Test]
        public void ShouldTruncateTheOverviewToFitTheCaptionLimit()
        {
            _episode.Overview = string.Join(" ", Enumerable.Repeat("overview", 400));

            var message = Build();

            message.HasPhoto.Should().BeTrue();
            message.Text.Should().Contain("…</blockquote>");

            // Markup is free, so compare against the text Telegram actually counts.
            StripMarkup(message.Text).Length.Should().BeLessOrEqualTo(1024);
        }

        [Test]
        public void ShouldKeepALongOverviewIntactWhenThereIsNoPhoto()
        {
            _settings.Image = (int)TelegramImageType.None;
            _episode.Overview = string.Join(" ", Enumerable.Repeat("overview", 400));

            Build().Text.Should().Contain(_episode.Overview);
        }

        [Test]
        public void ShouldJoinMultipleEpisodesInTheIdentifierAndTitle()
        {
            var second = new Episode { SeasonNumber = 1, EpisodeNumber = 2, Title = "Trust Fall", AirDate = "2026-08-23" };

            var message = TelegramMessageBuilder.BuildEpisodeMessage(
                "Episode Downloaded",
                _series,
                [_episode, second],
                new QualityModel(Quality.WEBDL1080p),
                null,
                [],
                _settings);

            message.Text.Should().StartWith("<b>Lanterns · S01E01-E02</b>\n<i>Pilot + Trust Fall</i>");
        }

        [Test]
        public void ShouldUseTheAirDateForDailySeries()
        {
            _series.SeriesType = SeriesTypes.Daily;

            Build().Text.Should().StartWith("<b>Lanterns · 2026-08-16</b>");
        }

        [Test]
        public void ShouldOmitTheRatingWhenTheSeriesIsUnrated()
        {
            _series.Ratings = new Ratings { Votes = 0, Value = 0m };

            Build().Text.Should().Contain("\n16 Aug 2026\n").And.NotContain("/10");
        }

        [Test]
        public void ShouldOmitTheSizeWhenItIsUnknown()
        {
            Build(size: null).Text.Should().Contain("<i>WEBDL-1080p Proper</i>");
        }

        [Test]
        public void ShouldEscapeHtmlInSeriesAndEpisodeTitles()
        {
            _series.Title = "Bob & Ted";
            _episode.Title = "<Pilot>";

            Build().Text.Should().StartWith("<b>Bob &amp; Ted · S01E01</b>\n<i>&lt;Pilot&gt;</i>");
        }

        [System.Text.RegularExpressions.GeneratedRegex("<.*?>")]
        private static partial System.Text.RegularExpressions.Regex HtmlTagMatcher();

        private static string StripMarkup(string text)
        {
            return HtmlTagMatcher().Replace(text, string.Empty)
                                                       .Replace("&amp;", "&", System.StringComparison.OrdinalIgnoreCase)
                                                       .Replace("&lt;", "<", System.StringComparison.OrdinalIgnoreCase)
                                                       .Replace("&gt;", ">", System.StringComparison.OrdinalIgnoreCase);
        }

        private TelegramMessage Build(long? size = 4700000000L, QualityModel quality = null)
        {
            return TelegramMessageBuilder.BuildEpisodeMessage(
                "Episode Downloaded",
                _series,
                [_episode],
                quality ?? new QualityModel(Quality.WEBDL1080p, new Revision(2)),
                size,
                [],
                _settings);
        }
    }
}
