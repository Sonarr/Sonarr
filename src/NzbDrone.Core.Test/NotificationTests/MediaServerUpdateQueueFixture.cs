using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class MediaServerUpdateQueueFixture : CoreTest<MediaServerUpdateQueue<MediaServerUpdateQueueFixture, string>>
    {
        [Test]
        public void should_use_latest_series_and_preserve_accumulated_info()
        {
            var original = new Series { Id = 1, Path = @"C:\Test\TV\Old Title".AsOsAgnostic() };
            var updated = new Series { Id = 1, Path = @"C:\Test\TV\New Title".AsOsAgnostic() };
            List<UpdateQueueItem<string>> processed = null;

            Subject.Add("server", original, "Created");
            Subject.Add("server", updated, "Modified");
            Subject.Add("server", updated, "Modified");
            Subject.ProcessQueue("server", items => processed = items);

            processed.Should().ContainSingle();
            processed[0].Series.Should().BeSameAs(updated);
            processed[0].Info.Should().BeEquivalentTo(new[] { "Created", "Modified" });

            Subject.ProcessQueue("server", _ => Assert.Fail("The queue should have been drained"));
        }
    }
}
