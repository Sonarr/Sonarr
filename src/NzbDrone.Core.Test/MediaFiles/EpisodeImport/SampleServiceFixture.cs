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

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class SampleServiceFixture : CoreTest<DetectSample>
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
                                    Quality = new QualityModel(Quality.HDTV720p)
                                };
        }

        private void GivenFileSize(long size)
        {
            _localEpisode.Size = size;
        }

        private void GivenRuntime(int seconds)
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Returns(new TimeSpan(0, 0, seconds));
        }

        [Test]
        public void should_return_false_if_season_zero()
        {
            _localEpisode.Episodes[0].SeasonNumber = 0;
            ShouldBeFalse();
        }

        [Test]
        public void should_return_false_for_flv()
        {
            _localEpisode.Path = @"C:\Test\some.show.s01e01.flv";

            ShouldBeFalse();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_for_strm()
        {
            _localEpisode.Path = @"C:\Test\some.show.s01e01.strm";

            ShouldBeFalse();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_runtime()
        {
            GivenRuntime(120);
            GivenFileSize(1000.Megabytes());

            Subject.IsSample(_localEpisode.Series,
                             _localEpisode.Quality,
                             _localEpisode.Path,
                             _localEpisode.Size,
                             _localEpisode.SeasonNumber);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_return_true_if_runtime_is_less_than_minimum()
        {
            GivenRuntime(60);

            ShouldBeTrue();
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_than_minimum()
        {
            GivenRuntime(600);

            ShouldBeFalse();
        }

        [Test]
        public void should_fall_back_to_file_size_if_mediainfo_dll_not_found_acceptable_size()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Throws<DllNotFoundException>();

            GivenFileSize(1000.Megabytes());
            ShouldBeFalse();
        }

        [Test]
        public void should_fall_back_to_file_size_if_mediainfo_dll_not_found_undersize()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Throws<DllNotFoundException>();

            GivenFileSize(1.Megabytes());
            ShouldBeTrue();
        }

        [Test]
        public void should_not_treat_daily_episode_a_special()
        {
            GivenRuntime(600);
            _series.SeriesType = SeriesTypes.Daily;
            _localEpisode.Episodes[0].SeasonNumber = 0;
            ShouldBeFalse();
        }

        [Test]
        public void should_return_false_for_anime_speical()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _localEpisode.Episodes[0].SeasonNumber = 0;

            ShouldBeFalse();
        }

        private void ShouldBeTrue()
        {
            Subject.IsSample(_localEpisode.Series,
                                         _localEpisode.Quality,
                                         _localEpisode.Path,
                                         _localEpisode.Size,
                                         _localEpisode.SeasonNumber).Should().BeTrue();
        }

        private void ShouldBeFalse()
        {
            Subject.IsSample(_localEpisode.Series,
                             _localEpisode.Quality,
                             _localEpisode.Path,
                             _localEpisode.Size,
                             _localEpisode.SeasonNumber).Should().BeFalse();
        }
    }
}
