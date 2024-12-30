using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using Workarr.Download;
using Workarr.Download.History;
using Workarr.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality;
using Workarr.Parser.Model;
using Workarr.Qualities;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    [TestFixture]
    public class AugmentQualityFromReleaseNameFixture : CoreTest<AugmentQualityFromReleaseName>
    {
        private LocalEpisode _localEpisode;
        private DownloadClientItem _downloadClientItem;
        private ParsedEpisodeInfo _hdtvParsedEpisodeInfo;
        private ParsedEpisodeInfo _webdlParsedEpisodeInfo;

        [SetUp]
        public void Setup()
        {
            _hdtvParsedEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew()
                                                               .With(p => p.Quality =
                                                                   new QualityModel(Workarr.Qualities.Quality.HDTV720p))
                                                               .Build();

            _webdlParsedEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew()
                                                                .With(p => p.Quality =
                                                                    new QualityModel(Workarr.Qualities.Quality.WEBDL720p))
                                                                .Build();

            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                                                             .Build();
        }

        [Test]
        public void should_return_null_if_download_client_item_is_null()
        {
            Subject.AugmentQuality(_localEpisode, null).Should().BeNull();
        }

        [Test]
        public void should_return_null_if_no_grabbed_history()
        {
            Mocker.GetMock<IDownloadHistoryService>()
                  .Setup(s => s.GetLatestGrab(It.IsAny<string>()))
                  .Returns((DownloadHistory)null);

            Subject.AugmentQuality(_localEpisode, _downloadClientItem).Should().BeNull();
        }

        [TestCase("Series.Title.S01E01.1080p.WEB.x264", QualitySource.Web, Confidence.Tag, 1080, Confidence.Tag)]
        [TestCase("Series.Title.S01E01.WEB.x264", QualitySource.Web, Confidence.Tag, 480, Confidence.Fallback)]
        [TestCase("Series.Title.S01E01.720p.x264", QualitySource.Television, Confidence.Fallback, 720, Confidence.Tag)]
        public void should_return_augmented_quality(string title, QualitySource source, Confidence sourceConfidence, int resolution, Confidence resolutionConfidence)
        {
            Mocker.GetMock<IDownloadHistoryService>()
                  .Setup(s => s.GetLatestGrab(It.IsAny<string>()))
                  .Returns(Builder<DownloadHistory>.CreateNew()
                                                   .With(h => h.SourceTitle = title)
                                                   .Build());

            var result = Subject.AugmentQuality(_localEpisode, _downloadClientItem);

            result.Should().NotBe(null);
            result.Source.Should().Be(source);
            result.SourceConfidence.Should().Be(sourceConfidence);
            result.Resolution.Should().Be(resolution);
            result.ResolutionConfidence.Should().Be(resolutionConfidence);
        }
    }
}
