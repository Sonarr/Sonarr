using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    [TestFixture]
    public class AugmentQualityFromMediaInfoFixture : CoreTest<AugmentQualityFromMediaInfo>
    {
        [Test]
        public void should_return_null_if_media_info_is_null()
        {
            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = null)
                                                    .Build();

            Subject.AugmentQuality(localEpisode, null).Should().Be(null);
        }

        [Test]
        public void should_return_null_if_media_info_width_is_zero()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = 0)
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            Subject.AugmentQuality(localEpisode, null).Should().Be(null);
        }

        [TestCase(4096, 1, 2160)] // True 4K
        [TestCase(4000, 1, 2160)]
        [TestCase(3840, 1, 2160)] // 4K UHD
        [TestCase(3200, 1, 2160)]
        [TestCase(2000, 1, 1080)]
        [TestCase(1920, 1, 1080)] // Full HD
        [TestCase(1440, 1080, 1080)] // 4:3 FullHD
        [TestCase(1800, 1, 1080)]
        [TestCase(1490, 1, 720)]
        [TestCase(1280, 1, 720)] // HD
        [TestCase(1200, 1, 720)]
        [TestCase(1000, 1, 576)]
        [TestCase(720, 576, 576)]
        [TestCase(800, 1, 480)]
        [TestCase(720, 1, 480)] // SDTV
        [TestCase(600, 1, 480)]
        [TestCase(100, 1, 480)]
        public void should_return_closest_resolution(int mediaInfoWidth, int mediaInfoHeight, int expectedResolution)
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = mediaInfoWidth)
                                                   .With(m => m.Height = mediaInfoHeight)
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentQuality(localEpisode, null);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(expectedResolution);
            result.Source.Should().Be(QualitySource.Unknown);
        }

        [Test]
        public void should_include_source_if_extracted_from_title()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                .With(m => m.Width = 1920)
                .With(m => m.Height = 1080)
                .With(m => m.Title = "Series.Title.S01E05.WEB.x264-Sonarr")
                .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                .With(l => l.MediaInfo = mediaInfo)
                .Build();

            var result = Subject.AugmentQuality(localEpisode, null);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(1080);
            result.Source.Should().Be(QualitySource.Web);
        }

        [Test]
        public void should_have_unknown_source_if_no_source_extracted_from_title()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                .With(m => m.Width = 1920)
                .With(m => m.Height = 1080)
                .With(m => m.Title = "Series.Title.S01E05.x264-Sonarr")
                .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                .With(l => l.MediaInfo = mediaInfo)
                .Build();

            var result = Subject.AugmentQuality(localEpisode, null);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(1080);
            result.Source.Should().Be(QualitySource.Unknown);
        }

        [Test]
        public void should_include_source_for_576_if_extracted_from_title()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                .With(m => m.Width = 1024)
                .With(m => m.Height = 576)
                .With(m => m.Title = "Series.Title.S01E05.Bluray.x264-Sonarr")
                .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                .With(l => l.MediaInfo = mediaInfo)
                .Build();

            var result = Subject.AugmentQuality(localEpisode, null);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(576);
            result.Source.Should().Be(QualitySource.Bluray);
        }
    }
}
