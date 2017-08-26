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

            Subject.AugmentQuality(localEpisode).Should().Be(null);
        }

        [Test]
        public void should_return_null_if_media_info__width_is_zero()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = 0)
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            Subject.AugmentQuality(localEpisode).Should().Be(null);
        }

        [Test]
        public void should_have_2160_resolution_if_width_is_over_1920()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = 3840)
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentQuality(localEpisode);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(2160);
        }

        [Test]
        public void should_have_1080_resolution_if_width_is_over_1280()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = 1920)
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentQuality(localEpisode);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(1080);
        }

        [Test]
        public void should_have_720_resolution_if_width_is_over_854()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = 1280)
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentQuality(localEpisode);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(720);
        }

        [Test]
        public void should_have_480_resolution_if_width_is_over_0()
        {
            var mediaInfo = Builder<MediaInfoModel>.CreateNew()
                                                   .With(m => m.Width = 640)
                                                   .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                                                    .With(l => l.MediaInfo = mediaInfo)
                                                    .Build();

            var result = Subject.AugmentQuality(localEpisode);

            result.Should().NotBe(null);
            result.Resolution.Should().Be(480);
        }
    }
}
