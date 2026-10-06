using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Caching;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Caching
{
    [TestFixture]
    public class ClearExpiredCacheServiceFixture : CoreTest<ClearExpiredCacheService>
    {
        [Test]
        public void should_clear_expired_cache_entries()
        {
            var cache = Mocker.Resolve<ICacheManager>().GetCache<string>(GetType());

            cache.Set("expired", "old", TimeSpan.FromMilliseconds(1));
            cache.Set("fresh", "new", TimeSpan.FromMinutes(30));

            Thread.Sleep(50);

            Subject.Execute(new ClearExpiredCacheCommand());

            cache.Count.Should().Be(1);
            cache.Find("fresh").Should().Be("new");
        }
    }
}
