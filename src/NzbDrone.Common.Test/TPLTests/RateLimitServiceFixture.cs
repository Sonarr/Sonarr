using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.TPL;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.TPLTests
{
    [TestFixture]
    [Platform(Exclude = "MacOsX")]
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

        [Test]
        public void should_extend_subkey_delay()
        {
            GivenExisting("me", _epoch + TimeSpan.FromMilliseconds(200));
            GivenExisting("me-sub", _epoch + TimeSpan.FromMilliseconds(300));

            Subject.WaitAndPulse("me", "sub", TimeSpan.FromMilliseconds(100));

            (GetRateLimitStore()["me-sub"] - _epoch).Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(400));
        }

        [Test]
        public void should_honor_basekey_delay()
        {
            GivenExisting("me", _epoch + TimeSpan.FromMilliseconds(200));
            GivenExisting("me-sub", _epoch + TimeSpan.FromMilliseconds(0));

            Subject.WaitAndPulse("me", "sub", TimeSpan.FromMilliseconds(100));

            (GetRateLimitStore()["me-sub"] - _epoch).Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(200));
        }

        [Test]
        public void should_not_extend_basekey_delay()
        {
            GivenExisting("me", _epoch + TimeSpan.FromMilliseconds(200));
            GivenExisting("me-sub", _epoch + TimeSpan.FromMilliseconds(100));

            Subject.WaitAndPulse("me", "sub", TimeSpan.FromMilliseconds(100));

            (GetRateLimitStore()["me"] - _epoch).Should().BeCloseTo(TimeSpan.FromMilliseconds(200));
        }
    }
}
