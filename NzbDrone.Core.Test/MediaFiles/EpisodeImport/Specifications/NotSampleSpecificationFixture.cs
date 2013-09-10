using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

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
                                    Series = _series
                                };
        }

        private void WithDailySeries()
        {
            _series.SeriesType = SeriesTypes.Daily;
        }

        private void WithSeasonZero()
        {
            _localEpisode.Episodes[0].SeasonNumber = 0;
        }

        private void WithFileSize(long size)
        {
            _localEpisode.Size = size;
        }

        private void WithLength(int minutes)
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<String>()))
                  .Returns(new TimeSpan(0, 0, minutes, 0));
        }

        [Test]
        public void should_return_true_if_series_is_daily()
        {
            WithDailySeries();

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_season_zero()
        {
            WithSeasonZero();

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_undersize_and_under_length()
        {
            WithFileSize(10.Megabytes());
            WithLength(1);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_undersize()
        {
            WithFileSize(10.Megabytes());
            WithLength(10);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_under_length()
        {
            WithFileSize(100.Megabytes());
            WithLength(1);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_over_size_and_length()
        {
            WithFileSize(100.Megabytes());
            WithLength(10);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_not_check_lenght_if_file_is_large_enough()
        {
            WithFileSize(100.Megabytes());

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_log_error_if_run_time_is_0_and_under_sample_size()
        {
            WithFileSize(40.Megabytes());
            WithLength(0);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeFalse();
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_skip_check_for_flv_file()
        {
            _localEpisode.Path = @"C:\Test\some.show.s01e01.flv";

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }
    }
}
