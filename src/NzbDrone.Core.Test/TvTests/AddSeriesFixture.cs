using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class AddSeriesFixture : CoreTest<AddSeriesService>
    {
        private Series _fakeSeries;

        [SetUp]
        public void Setup()
        {
            _fakeSeries = Builder<Series>
                .CreateNew()
                .With(s => s.Path = null)
                .Build();
        }

        private void GivenValidSeries(int tvdbId)
        {
            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetSeriesInfo(tvdbId, Language.English, It.IsAny<string>()))
                  .Returns(new Tuple<Series, List<Episode>>(_fakeSeries, new List<Episode>()));
        }

        private void GivenValidPath()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetSeriesFolder(It.IsAny<Series>(), null))
                  .Returns<Series, NamingConfig>((c, n) => c.Title);

            Mocker.GetMock<IAddSeriesValidator>()
                  .Setup(s => s.Validate(It.IsAny<Series>()))
                  .Returns(new ValidationResult());
        }

        [Test]
        public void should_be_able_to_add_a_series_without_passing_in_title()
        {
            var newSeries = new Series
            {
                TvdbId = 1,
                RootFolderPath = @"C:\Test\TV",
                Language = Language.English
            };

            GivenValidSeries(newSeries.TvdbId);
            GivenValidPath();

            var series = Subject.AddSeries(newSeries);

            series.Title.Should().Be(_fakeSeries.Title);
        }

        [Test]
        public void should_have_proper_path()
        {
            var newSeries = new Series
                            {
                                TvdbId = 1,
                                RootFolderPath = @"C:\Test\TV",
                                Language = Language.English
                            };

            GivenValidSeries(newSeries.TvdbId);
            GivenValidPath();

            var series = Subject.AddSeries(newSeries);

            series.Path.Should().Be(Path.Combine(newSeries.RootFolderPath, _fakeSeries.Title));
        }

        [Test]
        public void should_throw_if_series_validation_fails()
        {
            var newSeries = new Series
            {
                TvdbId = 1,
                Path = @"C:\Test\TV\Title1",
                Language = Language.English
            };

            GivenValidSeries(newSeries.TvdbId);

            Mocker.GetMock<IAddSeriesValidator>()
                  .Setup(s => s.Validate(It.IsAny<Series>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddSeries(newSeries));
        }

        [Test]
        public void should_throw_if_series_cannot_be_found()
        {
            var newSeries = new Series
            {
                TvdbId = 1,
                Path = @"C:\Test\TV\Title1",
                Language = Language.English
            };

            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetSeriesInfo(newSeries.TvdbId, Language.English, It.IsAny<string>()))
                  .Throws(new SeriesNotFoundException(newSeries.TvdbId));

            Mocker.GetMock<IAddSeriesValidator>()
                  .Setup(s => s.Validate(It.IsAny<Series>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddSeries(newSeries));

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_fetch_series_info_using_the_language_on_the_series()
        {
            var newSeries = new Series
            {
                TvdbId = 1,
                RootFolderPath = @"C:\Test\TV",
                Language = Language.Spanish
            };

            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetSeriesInfo(newSeries.TvdbId, Language.Spanish, It.IsAny<string>()))
                  .Returns(new Tuple<Series, List<Episode>>(_fakeSeries, new List<Episode>()));

            GivenValidPath();

            var series = Subject.AddSeries(newSeries);

            series.Language.Should().Be(Language.Spanish);

            Mocker.GetMock<IProvideSeriesInfo>()
                  .Verify(s => s.GetSeriesInfo(newSeries.TvdbId, Language.Spanish, It.IsAny<string>()), Times.Once());
        }
    }
}
