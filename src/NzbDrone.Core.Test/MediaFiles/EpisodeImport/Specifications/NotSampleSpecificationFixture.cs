using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class NotSampleSpecificationFixture : CoreTest<NotSampleSpecification>
    {
        private Series _series;
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Standard)
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
                  .Setup(s => s.GetRunTime(It.IsAny<String>()))
                  .Returns(new TimeSpan(0, 0, seconds));
        }

        [Test]
        public void should_return_true_if_series_is_daily()
        {
            _series.SeriesType = SeriesTypes.Daily;
            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_season_zero()
        {
            _localEpisode.Episodes[0].SeasonNumber = 0;
            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_existing_file()
        {
            _localEpisode.ExistingFile = true;
            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_flv()
        {
            _localEpisode.Path = @"C:\Test\some.show.s01e01.flv";

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_run_runtime_check_on_linux()
        {
            LinuxOnly();
            GivenFileSize(1000.Megabytes());

            Subject.IsSatisfiedBy(_localEpisode);
            
            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<String>()), Times.Never());
        }

        [Test]
        public void should_run_runtime_check_on_windows()
        {
            WindowsOnly();

            GivenRuntime(120);
            GivenFileSize(1000.Megabytes());

            Subject.IsSatisfiedBy(_localEpisode);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<String>()), Times.Once());
        }

        [Test]
        public void should_return_false_if_runtime_is_less_than_minimum()
        {
            WindowsOnly();
            GivenRuntime(60);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_runtime_greater_than_than_minimum()
        {
            WindowsOnly();
            GivenRuntime(120);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_file_size_is_under_minimum()
        {
            LinuxOnly();

            GivenRuntime(120);
            GivenFileSize(20.Megabytes());

            Subject.IsSatisfiedBy(_localEpisode).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_file_size_is_under_minimum_for_larger_limits()
        {
            LinuxOnly();

            GivenRuntime(120);
            GivenFileSize(120.Megabytes());
            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeFalse();
        }
    }
}
