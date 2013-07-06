using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFileTests.EpisodeImportTests
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
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFileSize(It.IsAny<String>()))
                  .Returns(size);
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
    }
}
