using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore.PagingSpecExtensionsTests
{
    public class PagingOffsetFixture
    {
        [TestCase(1, 10, 0)]
        [TestCase(2, 10, 10)]
        [TestCase(3, 20, 40)]
        [TestCase(1, 100, 0)]
        public void should_calcuate_expected_offset(int page, int pageSize, int expected)
        {
            var pagingSpec = new PagingSpec<Episode>
                {
                    Page = page,
                    PageSize = pageSize,
                    SortDirection = SortDirection.Ascending,
                    SortKey = "AirDate"
                };

            pagingSpec.PagingOffset().Should().Be(expected);
        }
    }
}
