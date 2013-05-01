using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;

namespace NzbDrone.Common.Test.CacheTests
{
    [TestFixture]
    public class CachedManagerFixture
    {
        [Test]
        public void should_return_proper_type_of_cache()
        {
            var result = CacheManger.GetCache<DateTime>(typeof(string));

            result.Should().BeOfType<Cached<DateTime>>();
        }


        [Test]
        public void multiple_calls_should_get_the_same_cache()
        {
            var result1 = CacheManger.GetCache<DateTime>(typeof(string));
            var result2 = CacheManger.GetCache<DateTime>(typeof(string));

            result1.Should().BeSameAs(result2);
        }




    }
}