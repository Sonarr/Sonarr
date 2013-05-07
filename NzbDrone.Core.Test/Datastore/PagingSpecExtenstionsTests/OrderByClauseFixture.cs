using System.ComponentModel;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore.PagingSpecExtenstionsTests
{
    public class OrderByClauseFixture
    {
        [Test]
        public void Test()
        {
            var pagingSpec = new PagingSpec<Episode>
                {
                    Page = 1,
                    PageSize = 10,
                    SortDirection = ListSortDirection.Ascending,
                    SortKey = "AirDate"
                };

            pagingSpec.OrderByClause().Should().NotBeNullOrEmpty();
        }
    }
}
