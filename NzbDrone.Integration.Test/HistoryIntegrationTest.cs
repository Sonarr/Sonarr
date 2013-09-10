using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class HistoryIntegrationTest : IntegrationTest
    {
        [Test]
        public void history_should_be_empty()
        {
            var history = History.GetPaged(1,15,"date","desc");

            history.Records.Count.Should().Be(0);
            history.Page.Should().Be(1);
            history.PageSize.Should().Be(15);
            history.Records.Should().BeEmpty();
        }

    }
}