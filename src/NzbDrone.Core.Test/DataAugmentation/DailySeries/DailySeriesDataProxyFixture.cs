using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.DailySeries;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.DataAugmentation.DailySeries
{
    [TestFixture]
    [IntegrationTest]
    public class DailySeriesDataProxyFixture : CoreTest<DailySeriesDataProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [Test]
        public void should_get_list_of_daily_series()
        {
            var list = Subject.GetDailySeriesIds();
            list.Should().NotBeEmpty();
            list.Should().OnlyHaveUniqueItems();
        }
    }
}
