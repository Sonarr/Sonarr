using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.SeriesServiceTests
{
    [TestFixture]
    public class FindByTitleInexactFixture : CoreTest<SeriesService>
    {
        private void GivenSeries(params Series[] series)
        {
            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.FindByTitleInexact(It.IsAny<string>()))
                .Returns(new List<Series>(series));
        }

        [Test]
        public void should_return_series_when_multiple_match_only_by_original_title()
        {
            var first = new Series { Title = "Translated One", CleanTitle = "translatedone", CleanOriginalTitle = "kaguya" };
            var second = new Series { Title = "Translated Two", CleanTitle = "translatedtwo", CleanOriginalTitle = "railgun" };

            GivenSeries(first, second);

            Subject.FindByTitleInexact("Kaguya Railgun 1080p").Should().Be(first);
        }

        [Test]
        public void should_prefer_leftmost_match_across_title_and_original_title()
        {
            var first = new Series { Title = "Railgun", CleanTitle = "railgun" };
            var second = new Series { Title = "Translated", CleanTitle = "translated", CleanOriginalTitle = "kaguya" };

            GivenSeries(first, second);

            Subject.FindByTitleInexact("Kaguya Railgun 1080p").Should().Be(second);
        }
    }
}
