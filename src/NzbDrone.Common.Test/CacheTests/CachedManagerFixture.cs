using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.CacheTests
{
    [TestFixture]
    public class CachedManagerFixture : TestBase<ICacheManager>
    {
        [Test]
        public void should_return_proper_type_of_cache()
        {
            var result = Subject.GetCache<DateTime>(typeof(string));

            result.Should().BeOfType<Cached<DateTime>>();
        }

        [Test]
        public void multiple_calls_should_get_the_same_cache()
        {
            var result1 = Subject.GetCache<DateTime>(typeof(string));
            var result2 = Subject.GetCache<DateTime>(typeof(string));

            result1.Should().BeSameAs(result2);
        }

        [Test]
        public void should_clear_expired_items_from_all_caches()
        {
            var cache1 = Subject.GetCache<string>(typeof(string), "first");
            var cache2 = Subject.GetCache<string>(typeof(string), "second");

            cache1.Set("expired", "old", TimeSpan.FromMilliseconds(1));
            cache1.Set("fresh", "new", TimeSpan.FromMinutes(30));
            cache2.Set("expired", "old", TimeSpan.FromMilliseconds(1));

            Thread.Sleep(50);

            Subject.ClearExpired();

            cache1.Count.Should().Be(1);
            cache1.Find("fresh").Should().Be("new");
            cache2.Count.Should().Be(0);
        }

        [Test]
        public void should_clear_expired_with_dictionary_cache_without_ttl()
        {
            var dictionary = Subject.GetCacheDictionary<string>(typeof(string), "dict");

            dictionary.Update(new Dictionary<string, string> { { "key", "value" } });

            Subject.ClearExpired();

            dictionary.Count.Should().Be(1);
        }
    }
}
