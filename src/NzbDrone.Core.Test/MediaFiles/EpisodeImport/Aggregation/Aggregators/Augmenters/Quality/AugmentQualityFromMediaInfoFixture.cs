using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
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

        [TestCase(4096, 2160)] // True 4K
        [TestCase(4000, 2160)]
        [TestCase(3840, 2160)] // 4K UHD
        [TestCase(3200, 2160)]
        [TestCase(2000, 1080)]
        [TestCase(1920, 1080)] // Full HD
        [TestCase(1800, 1080)]
        [TestCase(1490, 720)]
        [TestCase(1280, 720)] // HD
        [TestCase(1200, 720)]
        [TestCase(800, 480)]
        [TestCase(720, 480)] // SDTV
        [TestCase(600, 480)]
        [TestCase(100, 480)]
        public void should_return_closest_resolution(int mediaInfoWidth, int expectedResolution)
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = mediaInfoWidth)
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentQuality(localEpisode, null);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(expectedResolution);
        }
    }
}
