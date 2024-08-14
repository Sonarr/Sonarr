using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class DetectSampleFixture : CoreTest<DetectSample>
    {
        private Series _series;
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Standard)
                                     .With(s => s.Runtime = 30)
                                     .Build();

            var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .Build()
                                           .ToList();

            _localEpisode = new LocalEpisode
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Episodes = episodes,
                                    Series = _series,
                                    Quality = new QualityModel(Quality.HDTV720p),
                                };
        }

        private void GivenRuntime(int seconds)
        {
            var runtime = new TimeSpan(0, 0, seconds);

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Returns(runtime);

            _localEpisode.MediaInfo = Builder<MediaInfoModel>.CreateNew().With(m => m.RunTime = runtime).Build();
        }

        [Test]
        public void should_return_false_if_season_zero()
        {
            _localEpisode.Episodes[0].SeasonNumber = 0;

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_false_for_flv()
        {
            _localEpisode.Path = @"C:\Test\some.show.s01e01.flv";

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.NotSample);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_for_strm()
        {
            _localEpisode.Path = @"C:\Test\some.show.s01e01.strm";

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.NotSample);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_runtime()
        {
            GivenRuntime(120);

            Subject.IsSample(_localEpisode.Series,
                             _localEpisode.Path,
                             _localEpisode.IsSpecial);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_return_true_if_runtime_is_less_than_minimum()
        {
            GivenRuntime(60);

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.Sample);
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_minimum()
        {
            GivenRuntime(600);

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_webisode_minimum()
        {
            _series.Runtime = 6;
            GivenRuntime(299);

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_anime_short_minimum()
        {
            _series.Runtime = 2;
            GivenRuntime(60);

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_true_if_runtime_less_than_anime_short_minimum()
        {
            _series.Runtime = 2;
            GivenRuntime(10);

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.Sample);
        }

        [Test]
        public void should_return_indeterminate_if_mediainfo_result_is_null()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Returns((TimeSpan?)null);

            Subject.IsSample(_localEpisode.Series,
                             _localEpisode.Path,
                             _localEpisode.IsSpecial).Should().Be(DetectSampleResult.Indeterminate);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_treat_daily_episode_a_special()
        {
            GivenRuntime(600);
            _series.SeriesType = SeriesTypes.Daily;
            _localEpisode.Episodes[0].SeasonNumber = 0;

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_false_for_anime_special()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _localEpisode.Episodes[0].SeasonNumber = 0;

            Subject.IsSample(_localEpisode.Series,
                _localEpisode.Path,
                _localEpisode.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_use_runtime_from_media_info()
        {
            GivenRuntime(120);

            _localEpisode.Series.Runtime = 30;
            _localEpisode.Episodes.First().Runtime = 30;

            Subject.IsSample(_localEpisode).Should().Be(DetectSampleResult.Sample);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_runtime_from_episode_over_series()
        {
            GivenRuntime(120);

            _localEpisode.Series.Runtime = 5;
            _localEpisode.Episodes.First().Runtime = 30;

            Subject.IsSample(_localEpisode).Should().Be(DetectSampleResult.Sample);
        }
    }
}
