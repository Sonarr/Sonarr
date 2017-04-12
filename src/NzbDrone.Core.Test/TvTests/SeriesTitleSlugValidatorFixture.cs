using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation.Validators;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class SeriesTitleSlugValidatorFixture : CoreTest<SeriesTitleSlugValidator>
    {
        private List<Series> _series;
        private TestValidator<Series> _validator;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateListOfSize(1)
                                     .Build()
                                     .ToList();

            _validator = new TestValidator<Series>
                            {
                                v => v.RuleFor(s => s.TitleSlug).SetValidator(Subject)
                            };

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetAllSeries())
                  .Returns(_series);
        }

        [Test]
        public void should_not_be_valid_if_there_is_an_existing_series_with_the_same_title_slug()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 100)
                                        .With(s => s.TitleSlug = _series.First().TitleSlug)
                                        .Build();

            _validator.Validate(series).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_be_valid_if_there_is_not_an_existing_series_with_the_same_title_slug()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(s => s.TitleSlug = "MyTitleSlug")
                                        .Build();

            _validator.Validate(series).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_be_valid_if_there_is_an_existing_series_with_a_null_title_slug()
        {
            _series.First().TitleSlug = null;

            var series = Builder<Series>.CreateNew()
                                        .With(s => s.TitleSlug = "MyTitleSlug")
                                        .Build();

            _validator.Validate(series).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_be_valid_when_updating_an_existing_series()
        {
            _validator.Validate(_series.First().JsonClone()).IsValid.Should().BeTrue();
        }
    }
}
