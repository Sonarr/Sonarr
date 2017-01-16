using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.TPL;
using NzbDrone.Test.Common;
using FluentAssertions;

namespace NzbDrone.Common.Test.TPLTests
{
    [TestFixture]
    public class RateLimitServiceFixture : TestBase<RateLimitService>
    {
        private DateTime _epoch;

        [SetUp]
        public void SetUp()
        {
            // Make sure it's there so we don't affect measurements.
            Subject.GetType();

            _epoch = DateTime.UtcNow;
        }

        private ConcurrentDictionary<string, DateTime> GetRateLimitStore()
        {
            var cache = Mocker.Resolve<ICacheManager>()
                .GetCache<ConcurrentDictionary<string, DateTime>>(typeof(RateLimitService), "rateLimitStore");

            return cache.Get("rateLimitStore", () => new ConcurrentDictionary<string, DateTime>());
        }

        private void GivenExisting(string key, DateTime dateTime)
        {
            GetRateLimitStore().AddOrUpdate(key, dateTime, (s, i) => dateTime);
        }

        [Test]
        public void should_not_delay_if_unset()
        {
            var watch = Stopwatch.StartNew();
            Subject.WaitAndPulse("me", TimeSpan.FromMilliseconds(100));
            watch.Stop();

            watch.ElapsedMilliseconds.Should().BeLessThan(100);
        }

        [Test]
        public void should_not_delay_unrelated_key()
        {
            GivenExisting("other", _epoch + TimeSpan.FromMilliseconds(200));

            var watch = Stopwatch.StartNew();
            Subject.WaitAndPulse("me", TimeSpan.FromMilliseconds(100));
            watch.Stop();

            watch.ElapsedMilliseconds.Should().BeLessThan(50);
        }

        [Test]
        [Retry(3)]
        public void should_wait_for_existing()
        {
            GivenExisting("me", _epoch + TimeSpan.FromMilliseconds(200));

            var watch = Stopwatch.StartNew();
            Subject.WaitAndPulse("me", TimeSpan.FromMilliseconds(400));
            watch.Stop();

            watch.ElapsedMilliseconds.Should().BeInRange(175, 250);
        }

        [Test]
        public void should_extend_delay()
        {
            GivenExisting("me", _epoch + TimeSpan.FromMilliseconds(200));

            Subject.WaitAndPulse("me", TimeSpan.FromMilliseconds(100));

            (GetRateLimitStore()["me"] - _epoch).Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(300));
        }

        [Test]
        public void should_add_delay()
        {
            Subject.WaitAndPulse("me", TimeSpan.FromMilliseconds(100));

            (GetRateLimitStore()["me"] - _epoch).Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(100));
        }
    }
}
